namespace Veloce.Knowledgebase.Services.User;

public interface IUserRepository
{
    Task<UserEntity?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    );

    Task<UserEntity?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        UserEntity user,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        UserEntity user,
        CancellationToken cancellationToken = default
    );
}
