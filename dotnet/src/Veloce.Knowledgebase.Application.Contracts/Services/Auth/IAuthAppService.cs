namespace Veloce.Knowledgebase.Services.Auth;

public interface IAuthAppService
{
    Task<GenericMessageResponseDto> SignUpAsync(
        SignUpRequestDto input,
        CancellationToken cancellationToken = default
    );

    Task<AuthResponseDto> LogInAsync(
        LogInRequestDto input, CancellationToken cancellationToken = default
    );
}
