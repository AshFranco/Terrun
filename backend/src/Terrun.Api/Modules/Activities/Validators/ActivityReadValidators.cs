using FluentValidation;
using Terrun.Api.Modules.Activities.Contracts;

namespace Terrun.Api.Modules.Activities.Validators;

internal sealed class ActivityListQueryValidator : AbstractValidator<ActivityListQuery>
{
    public ActivityListQueryValidator()
    {
        RuleFor(q => q.Limit).InclusiveBetween(1, ActivityListQuery.MaxLimit);
        RuleFor(q => q.Type).IsInEnum();
        RuleFor(q => q.To).GreaterThan(q => q.From).When(q => q.From is not null && q.To is not null);
        RuleFor(q => q.Cursor).Must(cursor => cursor is null || ActivityCursor.TryParse(cursor, out _)).WithMessage("The cursor is not valid.");
    }
}

internal sealed class UpdateActivityRequestValidator : AbstractValidator<UpdateActivityRequest>
{
    private const int _titleMaxLength = 80;
    private const int _descriptionMaxLength = 500;

    public UpdateActivityRequestValidator()
    {
        RuleFor(r => r.Title).MaximumLength(_titleMaxLength);
        RuleFor(r => r.Description).MaximumLength(_descriptionMaxLength);
        When(r => r.Has(nameof(UpdateActivityRequest.Type)), () => RuleFor(r => r.Type).NotNull().IsInEnum());
        When(r => r.Has(nameof(UpdateActivityRequest.Visibility)), () => RuleFor(r => r.Visibility).NotNull().IsInEnum());
    }
}
