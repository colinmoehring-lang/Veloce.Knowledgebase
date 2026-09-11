using Veloce.Knowledgebase.Services.User;

namespace Veloce.Knowledgebase.Services.Ride;

public class RideAppService(
    IRideRepository rideRepository,
    IUserRepository userRepository) : IRideAppService
{
    public async Task<Guid> StartRideAsync(
        Guid userId,
        StartRideDto input,
        CancellationToken cancellationToken = default)
    {
        var ride = new RideEntity
        {
            RideId = Guid.NewGuid(),
            UserId = userId,
            VehicleId = input.VehicleId,
            VehicleType = input.VehicleType,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
            Duration = 0,
            Distance = 0,
            HighestSpeed = 0,
            CoordinateHighestSpeed = string.Empty,
            AverageSpeed = 0,
            HighestLeanAngleLeft = 0,
            CoordinateHighestLeanAngleLeft = string.Empty,
            HighestLeanAngleRight = 0,
            CoordinateHighestLeanAngleRight = string.Empty,
            HighestGForce = 0
        };

        await rideRepository.AddRideAsync(
            ride,
            cancellationToken);

        return ride.RideId;
    }

    public async Task AddRidePointAsync(
        Guid rideId,
        AddRidePointDto input,
        CancellationToken cancellationToken = default)
    {
        var point = new RidePointEntity
        {
            RidePointId = Guid.NewGuid(),
            RideId = rideId,
            Timestamp = input.Timestamp,
            Coordinate = input.Coordinate,
            Speed = input.Speed,
            GForce = input.GForce,
            Lean = input.Lean
        };

        await rideRepository.AddRidePointAsync(
            point,
            cancellationToken);
    }

    public async Task<RideDto?> StopRideAsync(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        var ride = await rideRepository.GetRideByIdAsync(
            rideId,
            cancellationToken);

        if (ride == null || ride.UserId != userId)
        {
            return null;
        }

        var points = await rideRepository.GetPointsByRideIdAsync(
            rideId,
            cancellationToken);

        if (points.Count == 0)
        {
            return MapToDto(ride);
        }

        ride.EndTime = DateTime.UtcNow;
        ride.Duration = (decimal)(ride.EndTime - ride.StartTime).TotalSeconds;
        ride.AverageSpeed = points.Average(p => p.Speed);

        var maxSpeedPoint = points.OrderByDescending(p => p.Speed).First();
        ride.HighestSpeed = maxSpeedPoint.Speed;
        ride.CoordinateHighestSpeed = maxSpeedPoint.Coordinate;

        var maxGForcePoint = points.OrderByDescending(p => p.GForce).First();
        ride.HighestGForce = maxGForcePoint.GForce;

        if (ride.VehicleType.Equals("Motorcycle", StringComparison.OrdinalIgnoreCase))
        {
            var maxLeft = points.Where(p => p.Lean < 0).OrderBy(p => p.Lean).FirstOrDefault();
            if (maxLeft != null)
            {
                ride.HighestLeanAngleLeft = maxLeft.Lean;
                ride.CoordinateHighestLeanAngleLeft = maxLeft.Coordinate;
            }

            var maxRight = points.Where(p => p.Lean > 0).OrderByDescending(p => p.Lean).FirstOrDefault();
            if (maxRight != null)
            {
                ride.HighestLeanAngleRight = maxRight.Lean;
                ride.CoordinateHighestLeanAngleRight = maxRight.Coordinate;
            }
        }

        await rideRepository.UpdateRideAsync(
            ride,
            cancellationToken);

        await CheckAndUpdateUserRecordsAsync(
            userId,
            ride,
            cancellationToken);

        return MapToDto(ride);
    }

    public async Task<List<RideDto>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rides = await rideRepository.GetRidesByUserIdAsync(
            userId,
            cancellationToken);

        return rides.Select(MapToDto).ToList();
    }

    public async Task<RideDto?> GetByIdAsync(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        var ride = await rideRepository.GetRideByIdAsync(
            rideId,
            cancellationToken);

        if (ride == null || ride.UserId != userId)
        {
            return null;
        }

        return MapToDto(ride);
    }

    private async Task CheckAndUpdateUserRecordsAsync(
        Guid userId,
        RideEntity ride,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user == null) return;

        var isUpdated = false;

        if (!user.SpeedRecord.HasValue || ride.HighestSpeed > user.SpeedRecord.Value)
        {
            user.SpeedRecord = ride.HighestSpeed;
            isUpdated = true;
        }

        if (!user.GForceRecord.HasValue || ride.HighestGForce > user.GForceRecord.Value)
        {
            user.GForceRecord = ride.HighestGForce;
            isUpdated = true;
        }

        if (ride.VehicleType.Equals("Motorcycle", StringComparison.OrdinalIgnoreCase))
        {
            if (!user.LeanRecordLeft.HasValue || ride.HighestLeanAngleLeft < user.LeanRecordLeft.Value)
            {
                user.LeanRecordLeft = ride.HighestLeanAngleLeft;
                isUpdated = true;
            }

            if (!user.LeanRecordRight.HasValue || ride.HighestLeanAngleRight > user.LeanRecordRight.Value)
            {
                user.LeanRecordRight = ride.HighestLeanAngleRight;
                isUpdated = true;
            }
        }

        if (isUpdated)
        {
            await userRepository.UpdateAsync(
                user,
                cancellationToken);
        }
    }

    private static RideDto MapToDto(RideEntity entity) => new()
    {
        RideId = entity.RideId,
        UserId = entity.UserId,
        VehicleId = entity.VehicleId,
        VehicleType = entity.VehicleType,
        StartTime = entity.StartTime,
        EndTime = entity.EndTime,
        Duration = entity.Duration,
        Distance = entity.Distance,
        HighestSpeed = entity.HighestSpeed,
        CoordinateHighestSpeed = entity.CoordinateHighestSpeed,
        AverageSpeed = entity.AverageSpeed,
        HighestLeanAngleLeft = entity.HighestLeanAngleLeft,
        CoordinateHighestLeanAngleLeft = entity.CoordinateHighestLeanAngleLeft,
        HighestLeanAngleRight = entity.HighestLeanAngleRight,
        CoordinateHighestLeanAngleRight = entity.CoordinateHighestLeanAngleRight,
        HighestGForce = entity.HighestGForce
    };
}
