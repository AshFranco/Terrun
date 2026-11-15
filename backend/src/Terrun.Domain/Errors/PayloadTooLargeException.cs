namespace Terrun.Domain.Errors;

public sealed class PayloadTooLargeException(string message) : DomainException(ErrorCodes.PayloadTooLarge, message);
