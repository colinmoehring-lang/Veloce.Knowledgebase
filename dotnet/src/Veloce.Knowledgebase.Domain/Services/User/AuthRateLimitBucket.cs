namespace Veloce.Knowledgebase.Services.User;

public class AuthRateLimitBucket
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime WindowEndsAtUtc { get; set; }
}
