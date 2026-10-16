using System.ComponentModel.DataAnnotations;

namespace Veloce.Knowledgebase.Services.Auth;

public class SignUpRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters long.")]
    public string Password { get; set; } = string.Empty;
}
