namespace Veloce.Knowledgebase.Services.Vehicle;

public class VehicleAppService(
    IVehicleRepository vehicleRepository) : IVehicleAppService
{
    public async Task<VehicleDto> CreateAsync(
        Guid userId, CreateVehicleDto input,
        CancellationToken cancellationToken = default)
    {
        var entity = new VehicleEntity
        {
            VehicleId = Guid.NewGuid(),
            UserId = userId,
            Name = input.Name,
            Description = input.Description,
            VehicleType = input.VehicleType,
            ImageData = input.ImageData
        };

        await vehicleRepository.AddAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<VehicleDto?> UpdateAsync(
        Guid userId,
        Guid vehicleId,
        UpdateVehicleDto input,
        CancellationToken cancellationToken = default)
    {
        var entity = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (entity == null || entity.UserId != userId)
        {
            return null;
        }

        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.VehicleType = input.VehicleType;
        entity.ImageData = input.ImageData;

        await vehicleRepository.UpdateAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var entity = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (entity == null || entity.UserId != userId)
        {
            return false;
        }

        await vehicleRepository.DeleteAsync(entity, cancellationToken);
        return true;
    }

    public async Task<List<VehicleDto>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entities = await vehicleRepository.GetListByUserIdAsync(userId, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<VehicleDto?> GetByIdAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var entity = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (entity == null || entity.UserId != userId)
        {
            return null;
        }

        return MapToDto(entity);
    }

    private static VehicleDto MapToDto(VehicleEntity entity) => new()
    {
        VehicleId = entity.VehicleId,
        UserId = entity.UserId,
        Name = entity.Name,
        Description = entity.Description,
        VehicleType = entity.VehicleType,
        ImageData = entity.ImageData
    };
}
