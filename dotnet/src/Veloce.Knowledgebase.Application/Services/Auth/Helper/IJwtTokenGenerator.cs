using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresInSeconds) GenerateToken(UserEntity user);
}
