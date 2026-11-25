using System.Net;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Terrun.Api.Modules.Identity.Contracts;
using Terrun.Domain.Errors;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Identity.Services;

// RF-12 and Law 172-13: an account exists only if the runner accepted the current terms AND the current privacy
// policy, each with its own checkbox; the evidence keeps the exact version, when, how and from which IP
public sealed class LegalConsentService(TerrunDbContext db, TimeProvider clock)
{
    private const string _consentUniqueConstraint = "uk_consent_user_id_legal_document_id";

    // Current version of each type = the latest one already in effect according to the injectable clock
    public async Task<IReadOnlyList<LegalDocumentResponse>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var effective = await db.LegalDocuments.AsNoTracking()
            .Where(d => d.EffectiveFrom <= now)
            .Select(d => new LegalDocumentResponse(d.Id, d.Type, d.Version, d.Url, d.EffectiveFrom))
            .ToListAsync(cancellationToken);

        return effective
            .GroupBy(d => d.Type)
            .Select(g => g.OrderByDescending(d => d.EffectiveFrom)
                .ThenByDescending(d => d.Version, StringComparer.Ordinal)
                .First())
            .OrderBy(d => d.Type)
            .ToList();
    }

    public async Task<IReadOnlyList<Guid>> RequireCurrentVersionsAsync(IReadOnlyList<Guid> acceptedIds, CancellationToken cancellationToken)
    {
        var currentIds = (await GetCurrentAsync(cancellationToken)).Select(d => d.Id).ToList();
        if (currentIds.Count != Enum.GetValues<LegalDocumentType>().Length || currentIds.Except(acceptedIds).Any())
        {
            throw new InvalidRequestException(
                ErrorCodes.ConsentRequired,
                "The current terms and privacy policy must both be accepted.");
        }

        return currentIds;
    }

    // CTZ-17 (LLD §3.2): records acceptance of new legal versions from settings without overwriting prior evidence
    public async Task<IReadOnlyList<ConsentResponse>> AcceptAsync(
        Guid userId,
        AcceptConsentsRequest request,
        IPAddress? ip,
        CancellationToken cancellationToken)
    {
        var requestedIds = request.LegalDocumentIds!.Distinct().ToList();
        var now = clock.GetUtcNow();
        var effectiveCount = await db.LegalDocuments.AsNoTracking()
            .CountAsync(d => requestedIds.Contains(d.Id) && d.EffectiveFrom <= now, cancellationToken);
        if (effectiveCount != requestedIds.Count)
        {
            throw new FieldValidationException(
                "legalDocumentIds",
                "One or more legal documents do not exist or are not yet in effect.");
        }

        var alreadyAcceptedIds = await db.Consents.AsNoTracking()
            .Where(c => c.UserId == userId && requestedIds.Contains(c.LegalDocumentId))
            .Select(c => c.LegalDocumentId)
            .ToHashSetAsync(cancellationToken);

        var newDocumentIds = requestedIds.Where(id => !alreadyAcceptedIds.Contains(id)).ToList();
        if (newDocumentIds.Count > 0)
        {
            Record(userId, newDocumentIds, ConsentMethod.Settings, NormalizeOptionalText(request.AppVersion), ip);
            await SaveConsentsAsync(cancellationToken);
        }

        return await (
            from consent in db.Consents.AsNoTracking()
            join document in db.LegalDocuments.AsNoTracking() on consent.LegalDocumentId equals document.Id
            where consent.UserId == userId && requestedIds.Contains(consent.LegalDocumentId)
            orderby document.Type, document.EffectiveFrom
            select new ConsentResponse(
                consent.LegalDocumentId,
                document.Type,
                document.Version,
                consent.AcceptedAt,
                consent.Method,
                consent.AppVersion))
            .ToListAsync(cancellationToken);
    }

    public void Record(Guid userId, IEnumerable<Guid> legalDocumentIds, ConsentMethod method, string? appVersion, IPAddress? ip)
    {
        var now = clock.GetUtcNow();
        var normalizedVersion = NormalizeOptionalText(appVersion);
        foreach (var documentId in legalDocumentIds)
        {
            db.Consents.Add(new Consent
            {
                UserId = userId,
                LegalDocumentId = documentId,
                AcceptedAt = now,
                Method = method,
                AppVersion = normalizedVersion,
                Ip = ip,
            });
        }
    }

    // Two identical retries arriving together: the unique index keeps the first evidence row and this request reads it back
    private async Task SaveConsentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: _consentUniqueConstraint,
        })
        {
            db.ChangeTracker.Clear();
        }
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
