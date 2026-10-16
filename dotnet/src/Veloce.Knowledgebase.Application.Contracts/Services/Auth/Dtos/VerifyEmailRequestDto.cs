using System.ComponentModel.DataAnnotations;

namespace Veloce.Knowledgebase.Services.Auth;

public class VerifyEmailRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
    public string Code { get; set; } = string.Empty;
}
