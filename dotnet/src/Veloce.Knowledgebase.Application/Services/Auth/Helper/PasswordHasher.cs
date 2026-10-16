using Microsoft.AspNetCore.Identity;
using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<UserEntity> _hasher = new();

    public string HashPassword(
        UserEntity user,
        string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        UserEntity user,
        string passwordHash,
        string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, passwordHash, providedPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
