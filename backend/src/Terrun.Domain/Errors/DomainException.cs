namespace Terrun.Domain.Errors;

// Services throw these; the API turns each one into a ProblemDetails with its HTTP status and stable code (LLD §3.4)
public abstract class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
