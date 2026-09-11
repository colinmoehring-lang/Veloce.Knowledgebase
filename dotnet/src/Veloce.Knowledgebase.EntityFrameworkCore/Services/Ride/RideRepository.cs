using Microsoft.EntityFrameworkCore;

namespace Veloce.Knowledgebase.Services.Ride;

public class RideRepository(IVeloceDbContext dbContext) : IRideRepository
{
    public async Task AddRideAsync(
        RideEntity ride,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Rides.AddAsync(ride, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRideAsync(
        RideEntity ride,
        CancellationToken cancellationToken = default)
    {
        dbContext.Rides.Update(ride);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRidePointAsync(
        RidePointEntity ridePoint,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RidePoints.AddAsync(ridePoint, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<string>> GetAllRidePointCoordinatesByRideIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RidePoints
            .Where(p => p.RideId == rideId)
            .Select(p => p.Coordinate)
            .ToListAsync(cancellationToken);
    }

    public async Task<RidePointEntity> GetRidePointByRidePointIdAsync(
        Guid ridePointId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RidePoints
            .FirstOrDefaultAsync(p => p.RidePointId == ridePointId, cancellationToken);
    }

    public async Task<RideEntity?> GetRideByIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Rides.FirstOrDefaultAsync(r => r.RideId == rideId, cancellationToken);
    }

    public async Task<List<RidePointEntity>> GetPointsByRideIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RidePoints
            .Where(p => p.RideId == rideId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RideEntity>> GetRidesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Rides
            .Where(r => r.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
