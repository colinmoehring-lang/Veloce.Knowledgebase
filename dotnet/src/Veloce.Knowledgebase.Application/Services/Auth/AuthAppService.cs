using Microsoft.Extensions.Logging;
using Veloce.Knowledgebase.Services.Email;
using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Auth;

public class AuthAppService(
    IUserRepository userRepository,
    IPasswordValidator passwordValidator,
    IPasswordHasher passwordHasher,
    IVerificationCodeGenerator verificationCodeGenerator,
    IJwtTokenGenerator jwtTokenGenerator,
    IEmailSender emailSender,
    ILogger<AuthAppService> logger) : IAuthAppService
{
    private readonly IUserRepository _userRepository = userRepository
        ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IPasswordValidator _passwordValidator = passwordValidator
        ?? throw new ArgumentNullException(nameof(passwordValidator));
    private readonly IPasswordHasher _passwordHasher = passwordHasher
        ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IVerificationCodeGenerator _verificationCodeGenerator = verificationCodeGenerator
        ?? throw new ArgumentNullException(nameof(verificationCodeGenerator));
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator
        ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    private readonly IEmailSender _emailSender = emailSender
        ?? throw new ArgumentNullException(nameof(emailSender));
    private readonly ILogger<AuthAppService> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));


    public async Task<GenericMessageResponseDto> SignUpAsync(
        SignUpRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = _passwordValidator.ValidatePassword(input.Password);
        if(!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null)
        {
            if (existingUser.Status == UserAccountStatus.Active)
            {
                throw new InvalidOperationException("A user with this email address already exists.");
            }

            // If account is still pending verification, update password hash and resend verification code
            existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, input.Password);
            await _userRepository.UpdateUserAsync(existingUser, cancellationToken);

            await _userRepository.InvalidateVerificationCodesForUserAsync(existingUser.UserId, cancellationToken);
            var code = _verificationCodeGenerator.GenerateNumericCode();
            await _userRepository.AddVerificationCodeAsync(new EmailVerificationCodeEntity
            {
                UserId = existingUser.UserId,
                Code = code,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(UserConsts.VerificationCodeExpiryMinutes)
            }, cancellationToken);

            await _emailSender.SendVerificationCodeAsync(normalizedEmail, code, cancellationToken);
            return new GenericMessageResponseDto
            {
                Success = true,
                Message = "Registration pending. A new verification code has been sent to your email."
            };
        }

        var user = new UserEntity
        {
            Email = input.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Status = UserAccountStatus.PendingVerification,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);

        await _userRepository.AddUserAsync(user, cancellationToken);

        var verificationCode = _verificationCodeGenerator.GenerateNumericCode();
        await _userRepository.AddVerificationCodeAsync(new EmailVerificationCodeEntity
        {
            UserId = user.UserId,
            Code = verificationCode,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(UserConsts.VerificationCodeExpiryMinutes)
        }, cancellationToken);

        try
        {
            await _emailSender.SendVerificationCodeAsync(user.Email, verificationCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send initial verification email to {Email}", user.Email);
            // Don't fail registration completely, the user can re-trigger sign-up with pending status to resend
        }

        return new GenericMessageResponseDto
        {
            Success = true,
            Message = "User successfully registered. Please verify your email with the sent verification code."
        };
    }

    public async Task<GenericMessageResponseDto> VerifyEmailAsync(
        VerifyEmailRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Status == UserAccountStatus.Active)
        {
            return new GenericMessageResponseDto
            {
                Success = true,
                Message = "Email is already verified."
            };
        }

        var latestCode = await _userRepository.GetLatestActiveVerificationCodeAsync(user.UserId, cancellationToken)
            ?? throw new InvalidOperationException("No valid verification code found. Please request a new one.");

        if (latestCode.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification code has expired. Please request a new one.");
        }

        if (!string.Equals(latestCode.Code, input.Code.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid verification code.");
        }

        latestCode.IsUsed = true;
        await _userRepository.UpdateVerificationCodeAsync(latestCode, cancellationToken);
        await _userRepository.InvalidateVerificationCodesForUserAsync(user.UserId, cancellationToken);

        user.Status = UserAccountStatus.Active;
        await _userRepository.UpdateUserAsync(user, cancellationToken);
        await _userRepository.DeleteVerificationCodeAsync(input.Code, cancellationToken);

        return new GenericMessageResponseDto
        {
            Success = true,
            Message = "Email verified successfully. You can now log in."
        };
    }

    public async Task<AuthResponseDto> LogInAsync(
        LogInRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Status == UserAccountStatus.PendingVerification)
        {
            throw new UnauthorizedAccessException("Email is not verified. Please verify your email first.");
        }

        if (user.Status == UserAccountStatus.Locked || user.Status == UserAccountStatus.Disabled)
        {
            throw new UnauthorizedAccessException("This user account is locked or disabled.");
        }

        var isPasswordCorrect = _passwordHasher.VerifyPassword(user, user.PasswordHash, input.Password);
        if (!isPasswordCorrect)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user, cancellationToken);

        var (token, expiresIn) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            UserId = user.UserId,
            Email = user.Email
        };
    }

    public async Task<GenericMessageResponseDto> ForgotPasswordAsync(
        ForgotPasswordRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

        // Security practice: Always return success message to avoid email enumeration
        if (user == null || user.Status == UserAccountStatus.Disabled)
        {
            return new GenericMessageResponseDto
            {
                Success = true,
                Message = "If this email address exists, a password reset code has been sent."
            };
        }

        await _userRepository.InvalidatePasswordResetCodesForUserAsync(user.UserId, cancellationToken);

        var resetCode = _verificationCodeGenerator.GenerateNumericCode();
        await _userRepository.AddPasswordResetCodeAsync(new PasswordResetCodeEntity
        {
            UserId = user.UserId,
            Code = resetCode,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(UserConsts.PasswordResetCodeExpiryMinutes)
        }, cancellationToken);

        await _emailSender.SendPasswordResetCodeAsync(user.Email, resetCode, cancellationToken);

        return new GenericMessageResponseDto
        {
            Success = true,
            Message = "If this email address exists, a password reset code has been sent."
        };
    }

    public async Task<GenericMessageResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = _passwordValidator.ValidatePassword(input.NewPassword);
        if (!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        var activeCode = await _userRepository.GetLatestActivePasswordResetCodeAsync(user.UserId, cancellationToken)
            ?? throw new InvalidOperationException("No valid password reset code found. Please request a new one.");

        if (activeCode.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Password reset code has expired. Please request a new one.");
        }

        if (!string.Equals(activeCode.Code, input.Code.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid password reset code.");
        }

        activeCode.IsUsed = true;
        await _userRepository.UpdatePasswordResetCodeAsync(activeCode, cancellationToken);
        await _userRepository.InvalidatePasswordResetCodesForUserAsync(user.UserId, cancellationToken);

        user.PasswordHash = _passwordHasher.HashPassword(user, input.NewPassword);
        await _userRepository.UpdateUserAsync(user, cancellationToken);

        return new GenericMessageResponseDto
        {
            Success = true,
            Message = "Password reset successfully. You can now log in with your new password."
        };
    }
}
