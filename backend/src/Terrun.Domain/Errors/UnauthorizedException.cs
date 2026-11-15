namespace Terrun.Domain.Errors;

public sealed class UnauthorizedException(string code, string message) : DomainException(code, message);
