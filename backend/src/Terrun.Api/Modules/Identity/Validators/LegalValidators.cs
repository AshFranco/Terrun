using FluentValidation;
using Terrun.Api.Modules.Identity.Contracts;

namespace Terrun.Api.Modules.Identity.Validators;

internal sealed class AcceptConsentsRequestValidator : AbstractValidator<AcceptConsentsRequest>
{
    // Technical bound so an invalid payload cannot send thousands of ids in one query
    public const int MaxDocumentsPerRequest = 10;

    public AcceptConsentsRequestValidator()
    {
        RuleFor(r => r.LegalDocumentIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(ids => ids!.Count <= MaxDocumentsPerRequest)
            .WithMessage($"At most {MaxDocumentsPerRequest} legal documents can be accepted at once.")
            .Must(ids => ids!.All(id => id != Guid.Empty))
            .WithMessage("Legal document ids cannot be empty.");

        RuleFor(r => r.AppVersion).MaximumLength(AuthRules.AppVersionMaxLength);
    }
}
