namespace Veloce.Knowledgebase.Services.User;

public class UserAppService(IUserRepository userRepository) : IUserAppService
{
    public async Task<UserRecordsDto?> GetUserRecordsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserRecordsDto
        {
            SpeedRecord = user.SpeedRecord,
            LeanRecordLeft = user.LeanRecordLeft,
            LeanRecordRight = user.LeanRecordRight,
            GForceRecord = user.GForceRecord
        };
    }
}
