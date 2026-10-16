namespace Veloce.Knowledgebase.Services.Ride;

public interface IRidePointAppService
{
    Task<ICollection<string>> GetAllRidePointCoordinatesByRideId(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default
    );

    Task<RidePointDetailDto?> GetRidePointDetailByRidePointId(
        Guid userId,
        Guid ridePointId,
        CancellationToken cancellationToken = default
    );
}
