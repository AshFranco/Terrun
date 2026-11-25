namespace Terrun.Domain.Errors;

// 400 VALIDATION_ERROR for a field that only the service can check (it needs the database), with the same "errors" per
// field that FluentValidation writes, so the app handles both the same way
public sealed class FieldValidationException(string field, string message)
    : DomainException(ErrorCodes.ValidationError, message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]> { [field] = [message] };
}
