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

    [HttpGet("all/{rideId}")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(ICollection<string>))]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<ICollection<string>> GetAllRidePointCoordinate(
        Guid rideId,
        CancellationToken cancellationToken)
    {
        return await _ridePointAppService.GetAllRidePointCoordinatesByRideId(rideId, cancellationToken);
    }

    [HttpGet("{ridePointId}")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(RidePointDetailDto))]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<RidePointDetailDto> GetRidePointDetailByRidePointId(
        Guid ridePointId,
        CancellationToken cancellationToken)
    {
        return await _ridePointAppService.GetRidePointDetailByRidePointId(ridePointId, cancellationToken);
    }
}
