using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(UserEntity user);
}
