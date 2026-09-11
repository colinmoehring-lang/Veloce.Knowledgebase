namespace Veloce.Knowledgebase.Services.User;

public class UserEntity
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public required string UserName { get; set; }
    public required string PasswordHash { get; set; }
    public decimal? SpeedRecord { get; set; }
    public decimal? LeanRecord { get; set; }
    public decimal? GForceRecord { get; set; }
}
