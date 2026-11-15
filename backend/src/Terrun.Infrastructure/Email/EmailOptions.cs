namespace Terrun.Infrastructure.Email;

// LLD §8: SMTP works with any provider (Brevo, Resend, Gmail) by changing only the configuration. Without a host the
// API logs instead of sending, which is what local development and the tests use
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string? SmtpHost { get; init; }

    public int SmtpPort { get; init; } = 587;

    public string? SmtpUsername { get; init; }

    public string? SmtpPassword { get; init; }

    public string Sender { get; init; } = "no-reply@terrun.local";

    public string SenderName { get; init; } = "Terrun";

    public bool IsSmtpConfigured => !string.IsNullOrWhiteSpace(SmtpHost);
}
