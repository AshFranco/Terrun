namespace Terrun.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

// Kind tells the log-only sender what it may reveal: verification codes are shown only in Development
public sealed record EmailMessage(string To, string Subject, string TextBody, string HtmlBody, EmailKind Kind, string? SecretForDevelopment = null);

public enum EmailKind
{
    EmailVerification,
    PasswordReset,
}
