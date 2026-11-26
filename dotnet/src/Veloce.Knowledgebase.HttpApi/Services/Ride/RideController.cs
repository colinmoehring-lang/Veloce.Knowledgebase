using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Veloce.Knowledgebase.Services.Ride;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("/api/integration/v{version:apiVersion}/[controller]")]
public class RideController(IRideAppService rideAppService) : ControllerBase
{
    private Guid CurrentUserId => User.GetRequiredUserId();

    [HttpPost("start")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> StartRide(
        [FromBody] StartRideDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var rideId = await rideAppService.StartRideAsync(CurrentUserId, input, cancellationToken);
            return Ok(new { RideId = rideId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{rideId:guid}/point")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> AddRidePoint(
        Guid rideId,
        [FromBody] AddRidePointDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            await rideAppService.AddRidePointAsync(CurrentUserId, rideId, input, cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound("Ride not found.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (RideNotActiveException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{rideId:guid}/stop")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> StopRide(
        Guid rideId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await rideAppService.StopRideAsync(CurrentUserId, rideId, cancellationToken);
            if (result == null)
            {
                return NotFound("Ride not found or unauthorized access.");
            }

            return Ok(result);
        }
        catch (RideNotActiveException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await rideAppService.GetListByUserIdAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{rideId:guid}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> GetById(
        Guid rideId,
        CancellationToken cancellationToken)
    {
        var result = await rideAppService.GetByIdAsync(
            CurrentUserId,
            rideId,
            cancellationToken);

        if (result == null)
        {
            return NotFound("Ride not found or unauthorized access.");
        }

        return Ok(result);
    }
}
