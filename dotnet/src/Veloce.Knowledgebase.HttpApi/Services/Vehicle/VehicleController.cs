using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Veloce.Knowledgebase.Services.Vehicle;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("/api/integration/v{version:apiVersion}/[controller]")]
public class VehicleController(IVehicleAppService vehicleAppService) : ControllerBase
{
    private readonly IVehicleAppService _vehicleAppService = vehicleAppService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleDto input,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleAppService.CreateAsync(CurrentUserId, input, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVehicleDto input,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleAppService.UpdateAsync(CurrentUserId, id, input, cancellationToken);
        if (result == null) return NotFound("Vehicle not found or unauthorized access.");

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _vehicleAppService.DeleteAsync(CurrentUserId, id, cancellationToken);
        if (!success) return NotFound("Vehicle not found or unauthorized access.");

        return NoContent();
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
    public async Task<IActionResult> GetList(
        CancellationToken cancellationToken)
    {
        var result = await _vehicleAppService.GetListByUserIdAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest,
        Type = typeof(string))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(string))]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleAppService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        if (result == null) return NotFound("Vehicle not found or unauthorized access.");

        return Ok(result);
    }
}
