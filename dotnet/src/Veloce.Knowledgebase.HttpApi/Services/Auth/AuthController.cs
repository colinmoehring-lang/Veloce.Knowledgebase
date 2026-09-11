using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Veloce.Knowledgebase.Services.Auth;

[ApiVersion("1.0")]
[ApiController]
[Route("/api/integration/v{version:apiVersion}/[controller]")]
public class AuthController(IAuthAppService authAppService) : ControllerBase
{
    private readonly IAuthAppService _authAppService = authAppService;

    [HttpPost("sign-up")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(GenericMessageResponseDto))]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> SignUp(
        [FromBody] SignUpRequestDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authAppService.SignUpAsync(input, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthRateLimitPolicy")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(AuthResponseDto))]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> LogIn(
        [FromBody] LogInRequestDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authAppService.LogInAsync(input, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}
