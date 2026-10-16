using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public interface IPasswordHasher
{
    string HashPassword(
        UserEntity user,
        string password);

    bool VerifyPassword(
        UserEntity user,
        string passwordHash,
        string providedPassword);
}
