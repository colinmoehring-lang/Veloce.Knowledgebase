namespace Veloce.Knowledgebase.Services.Ride;

public class RidePointAppService(
    IRideRepository rideRepository) : IRidePointAppService
{
    public async Task<ICollection<string>> GetAllRidePointCoordinatesByRideId(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        return await rideRepository.GetAllRidePointCoordinatesByRideIdAsync(
            rideId,
            cancellationToken);
    }

    public async Task<RidePointDetailDto> GetRidePointDetailByRidePointId(
        Guid ridePointId,
        CancellationToken cancellationToken = default)
    {
        var ridePoint = await rideRepository.GetRidePointByRidePointIdAsync(
            ridePointId,
            cancellationToken);

        if (ridePoint is null)
        {
            throw new Exception($"Ride point with ID {ridePointId} not found.");
        }

        return new RidePointDetailDto
        {
            RidePointId = ridePoint.RidePointId,
            Speed = ridePoint.Speed,
            GForce = ridePoint.GForce,
            Lean = ridePoint.Lean
        };
    }
}
