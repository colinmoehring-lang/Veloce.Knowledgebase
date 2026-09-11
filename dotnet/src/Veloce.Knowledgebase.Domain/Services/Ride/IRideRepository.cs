namespace Veloce.Knowledgebase.Services.Ride;

public interface IRideRepository
{
    Task AddRideAsync(
        RideEntity ride,
        CancellationToken cancellationToken = default);

    Task UpdateRideAsync(
        RideEntity ride,
        CancellationToken cancellationToken = default);

    Task AddRidePointAsync(
        RidePointEntity ridePoint,
        CancellationToken cancellationToken = default);

    Task<ICollection<string>> GetAllRidePointCoordinatesByRideIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default
    );

    Task<RidePointEntity> GetRidePointByRidePointIdAsync(
        Guid ridePointId,
        CancellationToken cancellationToken = default
    );

    Task<RideEntity?> GetRideByIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default);

    Task<List<RidePointEntity>> GetPointsByRideIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default);

    Task<List<RideEntity>> GetRidesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
