namespace Veloce.Knowledgebase.Services.Email;

public interface IEmailSender
{
    Task SendVerificationCodeAsync(
        string email, string code,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetCodeAsync(
        string email, string code,
        CancellationToken cancellationToken = default);
}
