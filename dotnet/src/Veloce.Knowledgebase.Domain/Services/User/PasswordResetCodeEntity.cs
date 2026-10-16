namespace Veloce.Knowledgebase.Services.User;

public class PasswordResetCodeEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
