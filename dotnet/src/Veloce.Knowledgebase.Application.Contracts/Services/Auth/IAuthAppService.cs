namespace Veloce.Knowledgebase.Services.Auth;

public interface IAuthAppService
{
    Task<GenericMessageResponseDto> SignUpAsync(
        SignUpRequestDto input,
        CancellationToken cancellationToken = default);

    Task<GenericMessageResponseDto> VerifyEmailAsync(
        VerifyEmailRequestDto input, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> LogInAsync(
        LogInRequestDto input, CancellationToken cancellationToken = default);

    Task<GenericMessageResponseDto> ForgotPasswordAsync(
        ForgotPasswordRequestDto input, CancellationToken cancellationToken = default);

    Task<GenericMessageResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto input, CancellationToken cancellationToken = default);
}
