namespace Veloce.Knowledgebase.Services.Vehicle;

public interface IVehicleRepository
{
    Task AddAsync(
        VehicleEntity vehicle,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        VehicleEntity vehicle,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        VehicleEntity vehicle,
        CancellationToken cancellationToken = default
    );

    Task<VehicleEntity?> GetByIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default
    );

    Task<List<VehicleEntity>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
