using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Veloce.Knowledgebase.Services.Auth;

public class VerificationCodeHasher(IConfiguration configuration)
{
    public string Hash(Guid userId, string purpose, string code)
    {
        var secret = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var data = Encoding.UTF8.GetBytes($"{purpose}:{userId:D}:{code}");
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), data));
    }

    public bool Verify(Guid userId, string purpose, string code, string stored)
    {
        var expected = stored.Length == 6 ? code : Hash(userId, purpose, code);
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(stored));
    }
}
