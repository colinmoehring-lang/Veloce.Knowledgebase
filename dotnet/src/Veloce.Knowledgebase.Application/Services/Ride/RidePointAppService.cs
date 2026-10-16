namespace Veloce.Knowledgebase.Services.Ride;

public class RidePointAppService(
    IRideRepository rideRepository) : IRidePointAppService
{
    public async Task<ICollection<string>> GetAllRidePointCoordinatesByRideId(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        var ride = await rideRepository.GetRideByIdAsync(rideId, cancellationToken);
        if (ride == null || ride.UserId != userId)
        {
            throw new UnauthorizedAccessException("Ride not found or access denied.");
        }

        return await rideRepository.GetAllRidePointCoordinatesByRideIdAsync(
            rideId,
            cancellationToken);
    }

    public async Task<RidePointDetailDto?> GetRidePointDetailByRidePointId(
        Guid userId,
        Guid ridePointId,
        CancellationToken cancellationToken = default)
    {
        var ridePoint = await rideRepository.GetRidePointByRidePointIdAsync(
            ridePointId,
            cancellationToken);

        if (ridePoint is null)
        {
            return null;
        }

        var ride = await rideRepository.GetRideByIdAsync(ridePoint.RideId, cancellationToken);
        if (ride == null || ride.UserId != userId)
        {
            throw new UnauthorizedAccessException("Ride not found or access denied.");
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
