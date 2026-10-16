using Microsoft.EntityFrameworkCore;
using Veloce.Knowledgebase.Services.Ride;
using Veloce.Knowledgebase.Services.User;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Veloce.Knowledgebase;

public class VeloceDbContext(DbContextOptions<VeloceDbContext> options)
    : DbContext(options), IVeloceDbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<EmailVerificationCodeEntity> EmailVerificationCodes { get; set; }
    public DbSet<PasswordResetCodeEntity> PasswordResetCodes { get; set; }
    public DbSet<VehicleEntity> Vehicles { get; set; }
    public DbSet<RideEntity> Rides { get; set; }
    public DbSet<RidePointEntity> RidePoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.NormalizedEmail).IsUnique();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(UserConsts.MaxEmailLength);
            entity.Property(x => x.NormalizedEmail).IsRequired().HasMaxLength(UserConsts.MaxEmailLength);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<EmailVerificationCodeEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.IsUsed, x.ExpiresAtUtc });
            entity.Property(x => x.Code).IsRequired().HasMaxLength(16);
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<PasswordResetCodeEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.IsUsed, x.ExpiresAtUtc });
            entity.Property(x => x.Code).IsRequired().HasMaxLength(16);
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
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
