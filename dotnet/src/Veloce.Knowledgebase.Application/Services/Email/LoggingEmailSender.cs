using Microsoft.Extensions.Logging;

namespace Veloce.Knowledgebase.Services.Email;

public class LoggingEmailSender(
    ILogger<LoggingEmailSender> logger) : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger = logger;

    public Task SendVerificationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL DISPATCH] Verification code for {Email}: {Code}", email, code);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL DISPATCH] Password reset code for {Email}: {Code}", email, code);
        return Task.CompletedTask;
    }
}
