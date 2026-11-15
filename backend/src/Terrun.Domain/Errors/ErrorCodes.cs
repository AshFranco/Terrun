namespace Terrun.Domain.Errors;

// LLD §3.4: stable codes the app translates into Spanish messages. Never rename one: the app depends on it
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
    public const string VerificationCodeInvalid = "VERIFICATION_CODE_INVALID";
    public const string ConsentRequired = "CONSENT_REQUIRED";
    public const string Forbidden = "FORBIDDEN";
    public const string UserSuspended = "USER_SUSPENDED";
    public const string NotFound = "NOT_FOUND";
    public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
    public const string HandleTaken = "HANDLE_TAKEN";
    public const string ActivityOverlap = "ACTIVITY_OVERLAP";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
    public const string BboxTooLarge = "BBOX_TOO_LARGE";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string RateLimited = "RATE_LIMITED";
    public const string InternalError = "INTERNAL_ERROR";
}
