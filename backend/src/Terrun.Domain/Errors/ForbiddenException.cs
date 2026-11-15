namespace Terrun.Domain.Errors;

public sealed class ForbiddenException(string code, string message) : DomainException(code, message);
