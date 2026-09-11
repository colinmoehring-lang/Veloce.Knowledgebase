using Microsoft.EntityFrameworkCore;

namespace Veloce.Knowledgebase.Services.User;

public class UserRepository(
    IVeloceDbContext dbContext) : IUserRepository
{
    public async Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FindAsync([userId], cancellationToken);
    }

    public async Task UpdateAsync(
    UserEntity user,
    CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
