using System.ComponentModel.DataAnnotations;

namespace Veloce.Knowledgebase.Services.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reset code is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Reset code must be 6 digits.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
}
