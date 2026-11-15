namespace Terrun.Domain.Errors;

// 400 with a specific code, for requests that are well formed but cannot be accepted (e.g. a wrong verification code)
public sealed class InvalidRequestException(string code, string message) : DomainException(code, message);
