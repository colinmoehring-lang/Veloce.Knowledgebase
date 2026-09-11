namespace Veloce.Knowledgebase.Services.Ride;

public interface IRidePointAppService
{
    Task<ICollection<string>> GetAllRidePointCoordinatesByRideId(
        Guid rideId,
        CancellationToken cancellationToken = default
    );

    Task<RidePointDetailDto> GetRidePointDetailByRidePointId(
        Guid ridePointId,
        CancellationToken cancellationToken = default
    );
}
