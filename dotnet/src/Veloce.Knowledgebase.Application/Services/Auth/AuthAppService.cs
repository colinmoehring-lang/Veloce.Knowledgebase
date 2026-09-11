using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public class AuthAppService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator) : IAuthAppService
{
    public async Task<GenericMessageResponseDto> SignUpAsync(SignUpRequestDto input, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByUserNameAsync(input.UserName, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User name is already taken.");
        }

        var user = new UserEntity
        {
            UserId = Guid.NewGuid(),
            UserName = input.UserName,
            PasswordHash = passwordHasher.HashPassword(input.Password)
        };

        await userRepository.AddAsync(user, cancellationToken);

        return new GenericMessageResponseDto
        {
            Success = true,
            Message = "User successfully registered."
        };
    }

    public async Task<AuthResponseDto> LogInAsync(LogInRequestDto input, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUserNameAsync(input.UserName, cancellationToken);
        if (user == null || !passwordHasher.VerifyPassword(input.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = tokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            UserId = user.UserId,
            UserName = user.UserName
        };
    }
}
