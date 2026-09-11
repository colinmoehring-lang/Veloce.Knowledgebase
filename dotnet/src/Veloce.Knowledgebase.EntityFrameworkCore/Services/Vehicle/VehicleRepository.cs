using Microsoft.EntityFrameworkCore;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Veloce.Knowledgebase.Services.Vehicle;

public class VehicleRepository(IVeloceDbContext dbContext) : IVehicleRepository
{
    public async Task AddAsync(
        VehicleEntity vehicle,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        VehicleEntity vehicle,
        CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Update(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        VehicleEntity vehicle,
        CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Remove(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<VehicleEntity?> GetByIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<List<VehicleEntity>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
