namespace Veloce.Knowledgebase.Services.User;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<UserEntity?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<UserEntity?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task AddUserAsync(
        UserEntity user,
        CancellationToken cancellationToken = default);

    Task UpdateUserAsync(
        UserEntity user,
        CancellationToken cancellationToken = default);

    Task AddVerificationCodeAsync(
        EmailVerificationCodeEntity codeEntity,
        CancellationToken cancellationToken = default);

    Task DeleteVerificationCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<EmailVerificationCodeEntity?> GetLatestActiveVerificationCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task InvalidateVerificationCodesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdateVerificationCodeAsync(
        EmailVerificationCodeEntity codeEntity,
        CancellationToken cancellationToken = default);

    Task AddPasswordResetCodeAsync(
        PasswordResetCodeEntity codeEntity,
        CancellationToken cancellationToken = default);

    Task<PasswordResetCodeEntity?> GetLatestActivePasswordResetCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task InvalidatePasswordResetCodesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdatePasswordResetCodeAsync(
        PasswordResetCodeEntity codeEntity,
        CancellationToken cancellationToken = default);
}
