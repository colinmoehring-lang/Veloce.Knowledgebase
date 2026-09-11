using System.ComponentModel.DataAnnotations;

namespace Veloce.Knowledgebase.Services.Auth;

public class SignUpRequestDto
{
    [Required(ErrorMessage = "User name is required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
