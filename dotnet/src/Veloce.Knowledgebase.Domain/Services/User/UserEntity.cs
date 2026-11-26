namespace Veloce.Knowledgebase.Services.User;

public class UserEntity
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();
    public UserAccountStatus Status { get; set; } = UserAccountStatus.PendingVerification;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }
    public decimal? SpeedRecord { get; set; }
    public decimal? LeanRecordLeft { get; set; }
    public decimal? LeanRecordRight { get; set; }
    public decimal? GForceRecord { get; set; }
}
