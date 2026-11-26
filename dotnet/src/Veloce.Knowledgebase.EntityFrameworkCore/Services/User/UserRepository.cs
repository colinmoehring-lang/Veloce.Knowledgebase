using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Veloce.Knowledgebase.Services.User;

public class UserRepository(
    VeloceDbContext dbContext) : IUserRepository
{
    private readonly IVeloceDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<IAuthTransaction> BeginAuthOperationAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({normalizedEmail}, 0))",
                cancellationToken);
            return new AuthTransaction(transaction);
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    private sealed class AuthTransaction(IDbContextTransaction transaction) : IAuthTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            transaction.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }

    public async Task<UserEntity?> GetByIdForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .FromSqlInterpolated($"SELECT * FROM \"Users\" WHERE \"UserId\" = {userId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FindAsync([userId], cancellationToken);
    }

    public async Task<UserEntity?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
    }

    public async Task<UserEntity?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
       string email,
       CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public async Task AddUserAsync(
        UserEntity user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(
        UserEntity user,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Entry(user).State == EntityState.Detached)
        {
            _dbContext.Users.Update(user);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddVerificationCodeAsync(
        EmailVerificationCodeEntity codeEntity, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailVerificationCodes.AddAsync(codeEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailVerificationCodeEntity?> GetLatestActiveVerificationCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailVerificationCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidateVerificationCodesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activeCodes = await _dbContext.EmailVerificationCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var code in activeCodes)
        {
            code.IsUsed = true;
        }

        if (activeCodes.Count != 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddPasswordResetCodeAsync(
        PasswordResetCodeEntity codeEntity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PasswordResetCodes.AddAsync(codeEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetCodeEntity?> GetLatestActivePasswordResetCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordResetCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidatePasswordResetCodesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activeCodes = await _dbContext.PasswordResetCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var code in activeCodes)
        {
            code.IsUsed = true;
        }

        if (activeCodes.Count != 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateVerificationCodeAsync(
        EmailVerificationCodeEntity codeEntity,
        CancellationToken cancellationToken = default)
    {
        _dbContext.EmailVerificationCodes.Update(codeEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePasswordResetCodeAsync(
        PasswordResetCodeEntity codeEntity,
        CancellationToken cancellationToken = default)
    {
        _dbContext.PasswordResetCodes.Update(codeEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
