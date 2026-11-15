using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Terrun.Infrastructure.Email;

// Used when no SMTP host is configured. LLD §7.3 forbids tokens and emails in the logs, so outside Development it
// records only that a message was skipped; in Development it shows the code so a developer can finish the flow
internal sealed partial class LogEmailSender(IHostEnvironment environment, ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment() && message.SecretForDevelopment is not null)
        {
            LogDevelopmentEmail(logger, message.Kind, message.SecretForDevelopment);
        }
        else
        {
            LogSkippedEmail(logger, message.Kind);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Email not sent (no SMTP configured). {Kind} code for local testing: {Code}")]
    private static partial void LogDevelopmentEmail(ILogger logger, EmailKind kind, string code);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Email not sent: no SMTP host is configured ({Kind})")]
    private static partial void LogSkippedEmail(ILogger logger, EmailKind kind);
}
