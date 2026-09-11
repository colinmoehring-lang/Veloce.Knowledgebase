using Microsoft.EntityFrameworkCore;
using Veloce.Knowledgebase.Services.Ride;
using Veloce.Knowledgebase.Services.User;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Veloce.Knowledgebase;

public class VeloceDbContext(DbContextOptions<VeloceDbContext> options)
    : DbContext(options), IVeloceDbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<VehicleEntity> Vehicles { get; set; }
    public DbSet<RideEntity> Rides { get; set; }
    public DbSet<RidePointEntity> RidePoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.UserName).IsUnique();
        });

        modelBuilder.Entity<VehicleEntity>(entity =>
        {
            entity.HasKey(e => e.VehicleId);
            entity.HasOne<UserEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RideEntity>(entity =>
        {
            entity.HasKey(e => e.RideId);
            entity.HasOne<UserEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<VehicleEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.VehicleId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RidePointEntity>(entity =>
        {
            entity.HasKey(e => e.RidePointId);
            entity.HasOne<RideEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.RideId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
