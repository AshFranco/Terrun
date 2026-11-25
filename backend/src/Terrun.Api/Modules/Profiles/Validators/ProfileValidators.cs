using System.Text.RegularExpressions;
using FluentValidation;
using Terrun.Api.Modules.Identity.Validators;
using Terrun.Api.Modules.Profiles.Contracts;

namespace Terrun.Api.Modules.Profiles.Validators;

// LLD §2.3 (M2) and D-32. Only the fields present in the body are checked; a null clears an optional field but can never
// clear the display name, the handle, the sex or the privacy flag
internal static class ProfileRules
{
    public const int BioMaxLength = 300;
    public const int CityMaxLength = 80;

    // Same bounds as the CHECK of profile.weight_kg
    public const decimal MinWeightKg = 25m;
    public const decimal MaxWeightKg = 300m;
    public const int MaxWeightDecimals = 2;

    public const short MinHeightCm = 100;
    public const short MaxHeightCm = 250;

    // D-28: Terrun is for adults only, so a birth date must say 18 or older
    public const int MinimumAgeYears = 18;
    public static readonly DateOnly EarliestBirthDate = new(1900, 1, 1);

    public const string CountryPattern = "^[A-Z]{2}$";

    public static string? NormalizeOptionalText(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    public static string? NormalizeCountry(string? country) => NormalizeOptionalText(country)?.ToUpperInvariant();
}

internal sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator(TimeProvider clock)
    {
        When(r => r.Has(nameof(UpdateProfileRequest.DisplayName)), () =>
            RuleFor(r => r.DisplayName)
                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("The display name is required.")
                .MaximumLength(AuthRules.DisplayNameMaxLength));

        When(r => r.Has(nameof(UpdateProfileRequest.Handle)), () =>
            RuleFor(r => r.Handle)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(handle => Regex.IsMatch(AuthRules.NormalizeHandle(handle!), AuthRules.HandlePattern))
                .WithMessage("The handle must have 3 to 30 letters, digits, '_' or '.'."));

        RuleFor(r => r.Bio).MaximumLength(ProfileRules.BioMaxLength);
        RuleFor(r => r.City).MaximumLength(ProfileRules.CityMaxLength);
        RuleFor(r => ProfileRules.NormalizeCountry(r.Country))
            .Matches(ProfileRules.CountryPattern)
            .OverridePropertyName(nameof(UpdateProfileRequest.Country))
            .WithMessage("The country must be an ISO 3166-1 alpha-2 code, such as DO.");

        RuleFor(r => r.BirthDate)
            .GreaterThanOrEqualTo(ProfileRules.EarliestBirthDate)
            .Must(birthDate => birthDate is null || IsAdult(birthDate.Value, clock))
            .WithMessage($"The runner must be at least {ProfileRules.MinimumAgeYears} years old.");

        When(r => r.Has(nameof(UpdateProfileRequest.Sex)), () =>
            RuleFor(r => r.Sex).NotNull().IsInEnum());

        RuleFor(r => r.WeightKg)
            .InclusiveBetween(ProfileRules.MinWeightKg, ProfileRules.MaxWeightKg)
            .PrecisionScale(5, ProfileRules.MaxWeightDecimals, ignoreTrailingZeros: true);

        RuleFor(r => r.HeightCm).InclusiveBetween(ProfileRules.MinHeightCm, ProfileRules.MaxHeightCm);

        When(r => r.Has(nameof(UpdateProfileRequest.IsPrivate)), () => RuleFor(r => r.IsPrivate).NotNull());
    }

    private static bool IsAdult(DateOnly birthDate, TimeProvider clock)
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        return birthDate <= today.AddYears(-ProfileRules.MinimumAgeYears);
    }
}
