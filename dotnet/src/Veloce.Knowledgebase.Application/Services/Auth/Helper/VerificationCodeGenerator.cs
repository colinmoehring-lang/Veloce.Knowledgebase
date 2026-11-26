using System.Security.Cryptography;

namespace Veloce.Knowledgebase.Services.Auth;

public class VerificationCodeGenerator : IVerificationCodeGenerator
{
    public string GenerateNumericCode(int length = 6)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
        }

        return new string(chars);
    }
}
