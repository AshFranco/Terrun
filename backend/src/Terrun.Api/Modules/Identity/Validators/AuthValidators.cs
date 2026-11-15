using FluentValidation;
using Terrun.Api.Modules.Identity.Contracts;
using Terrun.Infrastructure.Security;

namespace Terrun.Api.Modules.Identity.Validators;

// LLD §7.1 and the contract of §3.3. Messages are for developers; the app shows its own Spanish text per field
internal static class AuthRules
{
    public const int EmailMaxLength = 254;
    public const int PasswordMinLength = 10;

    // An upper bound keeps PBKDF2 from hashing megabytes sent on purpose
    public const int PasswordMaxLength = 128;
    public const int DisplayNameMaxLength = 60;
    public const int DeviceMaxLength = 100;
    public const int AppVersionMaxLength = 20;

    // 3 to 30 lowercase letters, digits, "_" or "."; neither starts nor ends with "."
    public const string HandlePattern = @"^(?!\.)(?!.*\.$)[a-z0-9._]{3,30}$";

    public static string NormalizeHandle(string handle) => handle.Trim().TrimStart('@').ToLowerInvariant();
}

internal sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(ICommonPasswordList commonPasswords)
    {
        RuleFor(r => r.Email).NotEmpty().MaximumLength(AuthRules.EmailMaxLength).EmailAddress();
        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(AuthRules.PasswordMinLength)
            .MaximumLength(AuthRules.PasswordMaxLength)
            .Must(password => !commonPasswords.Contains(password)).WithMessage("The password is too common.");
        RuleFor(r => r.DisplayName).Must(name => !string.IsNullOrWhiteSpace(name)).MaximumLength(AuthRules.DisplayNameMaxLength);
        RuleFor(r => r.Handle).NotEmpty()
            .Must(handle => System.Text.RegularExpressions.Regex.IsMatch(AuthRules.NormalizeHandle(handle), AuthRules.HandlePattern))
            .WithMessage("The handle must have 3 to 30 letters, digits, '_' or '.'.");
        RuleFor(r => r.AcceptedLegalDocumentIds).NotEmpty();
        RuleFor(r => r.AppVersion).MaximumLength(AuthRules.AppVersionMaxLength);
        RuleFor(r => r.Device).MaximumLength(AuthRules.DeviceMaxLength);
    }
}

internal sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().MaximumLength(AuthRules.EmailMaxLength);
        RuleFor(r => r.Password).NotEmpty().MaximumLength(AuthRules.PasswordMaxLength);
        RuleFor(r => r.Device).MaximumLength(AuthRules.DeviceMaxLength);
    }
}

internal sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(r => r.RefreshToken).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Device).MaximumLength(AuthRules.DeviceMaxLength);
    }
}

internal sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator() => RuleFor(r => r.RefreshToken).NotEmpty().MaximumLength(200);
}

internal sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().MaximumLength(AuthRules.EmailMaxLength);
        RuleFor(r => r.Code).NotEmpty().Matches("^[0-9]{6}$");
    }
}

internal sealed class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator() => RuleFor(r => r.Email).NotEmpty().MaximumLength(AuthRules.EmailMaxLength);
}
