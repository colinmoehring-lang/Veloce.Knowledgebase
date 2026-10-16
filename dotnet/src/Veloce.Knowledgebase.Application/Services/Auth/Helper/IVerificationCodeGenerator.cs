namespace Veloce.Knowledgebase.Services.Auth;

public interface IVerificationCodeGenerator
{
    string GenerateNumericCode(int length = 6);
}
