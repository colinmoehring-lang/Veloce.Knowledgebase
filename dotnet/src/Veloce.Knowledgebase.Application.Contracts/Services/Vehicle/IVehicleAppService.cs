namespace Veloce.Knowledgebase.Services.Vehicle;

public interface IVehicleAppService
{
    Task<VehicleDto> CreateAsync(
        Guid userId,
        CreateVehicleDto input,
        CancellationToken cancellationToken = default
    );

    Task<VehicleDto?> UpdateAsync(
        Guid userId,
        Guid vehicleId,
        UpdateVehicleDto input,
        CancellationToken cancellationToken = default
    );

    Task<bool> DeleteAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default
    );

    Task<List<VehicleDto>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<VehicleDto?> GetByIdAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default
    );
}
