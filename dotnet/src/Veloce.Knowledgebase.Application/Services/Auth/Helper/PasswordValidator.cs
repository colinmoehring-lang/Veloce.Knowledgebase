using System.Text.RegularExpressions;
using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public class PasswordValidator : IPasswordValidator
{
    private static readonly Regex HasUpperCase = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex HasLowerCase = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex HasDigits = new(@"[0-9]", RegexOptions.Compiled);
    private static readonly Regex HasSpecialChars = new(@"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|~]", RegexOptions.Compiled);

    public (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty.");

        if (password.Length < UserConsts.MinPasswordLength)
            return (false, $"Password must be at least {UserConsts.MinPasswordLength} characters long.");

        if (password.Length > UserConsts.MaxPasswordLength)
            return (false, $"Password must not exceed {UserConsts.MaxPasswordLength} characters.");

        if (!HasUpperCase.IsMatch(password))
            return (false, "Password must contain at least one uppercase letter.");

        if (!HasLowerCase.IsMatch(password))
            return (false, "Password must contain at least one lowercase letter.");

        if (!HasDigits.IsMatch(password))
            return (false, "Password must contain at least one number.");

        if (!HasSpecialChars.IsMatch(password))
            return (false, "Password must contain at least one special character.");

        return (true, null);
    }
}
