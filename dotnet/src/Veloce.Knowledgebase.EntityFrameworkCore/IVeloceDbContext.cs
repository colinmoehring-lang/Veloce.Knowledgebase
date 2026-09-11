using Microsoft.EntityFrameworkCore;
using Veloce.Knowledgebase.Services.Ride;
using Veloce.Knowledgebase.Services.User;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Veloce.Knowledgebase;

public interface IVeloceDbContext
{
    DbSet<UserEntity> Users { get; set; }
    DbSet<VehicleEntity> Vehicles { get; set; }
    DbSet<RideEntity> Rides { get; set; }
    DbSet<RidePointEntity> RidePoints { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
