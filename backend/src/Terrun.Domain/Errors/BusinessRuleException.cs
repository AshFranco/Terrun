namespace Terrun.Domain.Errors;

public sealed class BusinessRuleException(string code, string message) : DomainException(code, message);
