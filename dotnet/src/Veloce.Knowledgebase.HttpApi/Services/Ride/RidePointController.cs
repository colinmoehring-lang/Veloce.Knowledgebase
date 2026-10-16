using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Veloce.Knowledgebase.Services.Ride;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("/api/integration/v{version:apiVersion}/[controller]")]
public class RidePointController(IRidePointAppService ridePointAppService) : ControllerBase
{
    private readonly IRidePointAppService _ridePointAppService = ridePointAppService;
    private Guid CurrentUserId => User.GetRequiredUserId();

    [HttpGet("all/{rideId}")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(ICollection<string>))]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status404NotFound,
        Type = typeof(string))]
    public async Task<IActionResult> GetAllRidePointCoordinate(
        Guid rideId,
        CancellationToken cancellationToken)
    {
        try
        {
            var coordinates = await _ridePointAppService.GetAllRidePointCoordinatesByRideId(CurrentUserId, rideId, cancellationToken);
            return Ok(coordinates);
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{ridePointId}")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(RidePointDetailDto))]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status404NotFound,
        Type = typeof(string))]
    public async Task<IActionResult> GetRidePointDetailByRidePointId(
        Guid ridePointId,
        CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _ridePointAppService.GetRidePointDetailByRidePointId(CurrentUserId, ridePointId, cancellationToken);
            if (detail == null)
            {
                return NotFound($"Ride point with ID {ridePointId} not found.");
            }

            return Ok(detail);
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
