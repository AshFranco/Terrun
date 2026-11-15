namespace Terrun.Domain.Errors;

// LLD §3.4: also used when the resource exists but is not visible to the caller, so nothing leaks
public sealed class NotFoundException(string message) : DomainException(ErrorCodes.NotFound, message);
