using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration = configuration;

    public (string Token, int ExpiresInSeconds) GenerateToken(UserEntity user)
    {
        var jwtKey = _configuration["Jwt:SecretKey"]
            ?? _configuration["Jwt:Key"]
            ?? "SuperSecretDefaultKeyForDevelopmentOnly123456!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "Veloce.Knowledgebase";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "Veloce.Knowledgebase.Clients";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("status", user.Status.ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds;

        return (tokenString, expiresInSeconds);
    }
}
