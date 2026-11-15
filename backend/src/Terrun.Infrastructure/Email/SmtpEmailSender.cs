using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Terrun.Infrastructure.Email;

// MailKit instead of System.Net.Mail.SmtpClient, which Microsoft no longer recommends for new code
internal sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var host = settings.SmtpHost ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
        var mime = new MimeMessage
        {
            Subject = message.Subject,
            Body = new BodyBuilder { TextBody = message.TextBody, HtmlBody = message.HtmlBody }.ToMessageBody(),
        };
        mime.From.Add(new MailboxAddress(settings.SenderName, settings.Sender));
        mime.To.Add(MailboxAddress.Parse(message.To));

        using var client = new SmtpClient();
        await client.ConnectAsync(host, settings.SmtpPort, SecureSocketOptions.Auto, cancellationToken);
        if (!string.IsNullOrEmpty(settings.SmtpUsername))
        {
            await client.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
