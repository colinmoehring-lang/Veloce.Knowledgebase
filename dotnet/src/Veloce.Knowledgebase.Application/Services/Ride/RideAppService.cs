using System.Globalization;
using Veloce.Knowledgebase.Services.User;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Veloce.Knowledgebase.Services.Ride;

public class RideAppService(
    IRideRepository rideRepository,
    IUserRepository userRepository,
    IVehicleRepository vehicleRepository) : IRideAppService
{
    public async Task<Guid> StartRideAsync(
        Guid userId,
        StartRideDto input,
        CancellationToken cancellationToken = default)
    {
        if (input.VehicleId is Guid vehicleId)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle == null || vehicle.UserId != userId)
            {
                throw new KeyNotFoundException("Vehicle not found.");
            }
        }

        var ride = new RideEntity
        {
            RideId = Guid.NewGuid(),
            UserId = userId,
            Status = RideStatus.Active,
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
        Guid userId,
        Guid rideId,
        AddRidePointDto input,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseCoordinate(input.Coordinate, out _, out _))
        {
            throw new ArgumentException("Coordinate must contain a valid latitude and longitude.");
        }

        if (input.Timestamp.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must include a UTC offset.");
        }

        var point = new RidePointEntity
        {
            RidePointId = Guid.NewGuid(),
            RideId = rideId,
            Timestamp = input.Timestamp.ToUniversalTime(),
            Coordinate = input.Coordinate,
            Speed = input.Speed,
            GForce = input.GForce,
            Lean = input.Lean
        };

        var result = await rideRepository.WithRideLockAsync(userId, rideId, async ride =>
        {
            if (ride.Status != RideStatus.Active)
            {
                throw new RideNotActiveException("Ride is not active.");
            }

            await rideRepository.AddRidePointAsync(point, cancellationToken);
            return point;
        }, cancellationToken);
        if (result == null)
        {
            throw new UnauthorizedAccessException("Ride not found or access denied.");
        }
    }

    public async Task<RideDto?> StopRideAsync(
        Guid userId,
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        return await rideRepository.WithRideLockAsync(userId, rideId, async ride =>
        {
            if (ride.Status == RideStatus.Completed)
            {
                return MapToDto(ride);
            }

            if (ride.Status != RideStatus.Active)
            {
                throw new RideNotActiveException("Ride status is unknown.");
            }

            var points = await rideRepository.GetPointsByRideIdAsync(rideId, cancellationToken);
            var now = DateTime.UtcNow;
            ride.EndTime = now.AddTicks(-(now.Ticks % 10));
            ride.Duration = (decimal)(ride.EndTime - ride.StartTime).TotalSeconds;
            ride.Status = RideStatus.Completed;
            if (points.Count == 0)
            {
                await rideRepository.UpdateRideAsync(ride, cancellationToken);
                return MapToDto(ride);
            }

            ride.Distance = CalculateDistance(points);
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

            await rideRepository.UpdateRideAsync(ride, cancellationToken);
            await CheckAndUpdateUserRecordsAsync(userId, ride, cancellationToken);
            return MapToDto(ride);
        }, cancellationToken);
    }

    private static decimal CalculateDistance(List<RidePointEntity> points)
    {
        const double earthRadiusMeters = 6371000;
        var distance = 0.0;
        (double Latitude, double Longitude)? previous = null;

        foreach (var point in points.OrderBy(p => p.Timestamp).ThenBy(p => p.RidePointId))
        {
            if (!TryParseCoordinate(point.Coordinate, out var latitude, out var longitude))
            {
                throw new InvalidOperationException($"Ride point {point.RidePointId} has an invalid coordinate.");
            }

            if (previous is { } last)
            {
                var lat1 = last.Latitude * Math.PI / 180;
                var lat2 = latitude * Math.PI / 180;
                var deltaLat = lat2 - lat1;
                var deltaLon = (longitude - last.Longitude) * Math.PI / 180;
                var a = Math.Pow(Math.Sin(deltaLat / 2), 2)
                    + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2), 2);
                distance += 2 * earthRadiusMeters * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
            }

            previous = (latitude, longitude);
        }

        return (decimal)distance;
    }

    private static bool TryParseCoordinate(string? coordinate, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;
        var parts = coordinate?.Split(',');
        return parts is { Length: 2 }
            && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude)
            && double.IsFinite(latitude) && latitude is >= -90 and <= 90
            && double.IsFinite(longitude) && longitude is >= -180 and <= 180;
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
        var user = await userRepository.GetByIdForUpdateAsync(
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
            if (ride.HighestLeanAngleLeft < 0 && (!user.LeanRecordLeft.HasValue || ride.HighestLeanAngleLeft < user.LeanRecordLeft.Value))
            {
                user.LeanRecordLeft = ride.HighestLeanAngleLeft;
                isUpdated = true;
            }

            if (ride.HighestLeanAngleRight > 0 && (!user.LeanRecordRight.HasValue || ride.HighestLeanAngleRight > user.LeanRecordRight.Value))
            {
                user.LeanRecordRight = ride.HighestLeanAngleRight;
                isUpdated = true;
            }
        }

        if (isUpdated)
        {
            await userRepository.UpdateUserAsync(
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
