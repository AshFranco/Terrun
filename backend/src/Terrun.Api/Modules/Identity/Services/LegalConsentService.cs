using System.Net;
using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Errors;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Identity.Services;

// RF-12 and Law 172-13: an account exists only if the runner accepted the current terms AND the current privacy
// policy, each with its own checkbox; the evidence keeps the exact version, when, how and from which IP
public sealed class LegalConsentService(TerrunDbContext db, TimeProvider clock)
{
    public async Task<IReadOnlyList<Guid>> RequireCurrentVersionsAsync(IReadOnlyList<Guid> acceptedIds, CancellationToken cancellationToken)
    {
        var currentIds = await CurrentDocumentIdsAsync(cancellationToken);
        if (currentIds.Count != Enum.GetValues<LegalDocumentType>().Length || currentIds.Except(acceptedIds).Any())
        {
            throw new InvalidRequestException(
                ErrorCodes.ConsentRequired,
                "The current terms and privacy policy must both be accepted.");
        }

        return currentIds;
    }

    public void Record(Guid userId, IEnumerable<Guid> legalDocumentIds, ConsentMethod method, string? appVersion, IPAddress? ip)
    {
        var now = clock.GetUtcNow();
        foreach (var documentId in legalDocumentIds)
        {
            db.Consents.Add(new Consent
            {
                UserId = userId,
                LegalDocumentId = documentId,
                AcceptedAt = now,
                Method = method,
                AppVersion = appVersion,
                Ip = ip,
            });
        }
    }

    // Current version of each type = the latest one already in effect
    private async Task<List<Guid>> CurrentDocumentIdsAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await db.LegalDocuments
            .Where(d => d.EffectiveFrom <= now)
            .GroupBy(d => d.Type)
            .Select(g => g.OrderByDescending(d => d.EffectiveFrom).Select(d => d.Id).First())
            .ToListAsync(cancellationToken);
    }
}
