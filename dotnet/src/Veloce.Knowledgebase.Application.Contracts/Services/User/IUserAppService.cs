namespace Veloce.Knowledgebase.Services.User;

public interface IUserAppService
{
    Task<UserRecordsDto?> GetUserRecordsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
