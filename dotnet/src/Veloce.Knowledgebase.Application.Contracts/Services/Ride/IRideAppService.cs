namespace Veloce.Knowledgebase.Services.Ride;

public interface IRideAppService
{
    Task<Guid> StartRideAsync(
        Guid userId,
        StartRideDto input,
        CancellationToken cancellationToken = default);

    Task AddRidePointAsync(
        Guid rideId,
        AddRidePointDto input,
        CancellationToken cancellationToken = default);

    Task<RideDto?> StopRideAsync(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default);

    Task<List<RideDto>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<RideDto?> GetByIdAsync(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default);
}
