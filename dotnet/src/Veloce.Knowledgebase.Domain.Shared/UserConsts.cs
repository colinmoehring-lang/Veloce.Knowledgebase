namespace Veloce.Knowledgebase.Services.User;

public static class UserConsts
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;
    public const int VerificationCodeExpiryMinutes = 15;
    public const int PasswordResetCodeExpiryMinutes = 15;
    public const int MaxEmailLength = 256;
}
