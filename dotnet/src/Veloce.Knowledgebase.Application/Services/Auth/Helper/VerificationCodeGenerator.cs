using System.Security.Cryptography;

namespace Veloce.Knowledgebase.Services.Auth;

public class VerificationCodeGenerator : IVerificationCodeGenerator
{
    public string GenerateNumericCode(int length = 6)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)('0' + (bytes[i] % 10));
        }

        return new string(chars);
    }
}
