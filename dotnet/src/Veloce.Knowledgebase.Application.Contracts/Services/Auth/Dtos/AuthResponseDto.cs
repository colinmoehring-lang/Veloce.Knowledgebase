namespace Veloce.Knowledgebase.Services.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
