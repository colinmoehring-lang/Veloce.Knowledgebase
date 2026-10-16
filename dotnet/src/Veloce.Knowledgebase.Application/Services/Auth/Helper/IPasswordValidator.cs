namespace Veloce.Knowledgebase.Services.Auth;

public interface IPasswordValidator
{
    (bool IsValid, string? ErrorMessage) ValidatePassword(string password);
}
