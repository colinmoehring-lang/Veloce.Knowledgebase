namespace Veloce.Knowledgebase.Services.Ride;

public class RideEntity
{
    public required Guid RideId { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public Guid? VehicleId { get; set; }
    public required string VehicleType { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required decimal Duration { get; set; }
    public required decimal Distance { get; set; }
    public required decimal HighestSpeed { get; set; }
    public required string CoordinateHighestSpeed { get; set; }
    public required decimal AverageSpeed { get; set; }
    public required decimal HighestLeanAngleLeft { get; set; }
    public required string CoordinateHighestLeanAngleLeft { get; set; }
    public required decimal HighestLeanAngleRight { get; set; }
    public required string CoordinateHighestLeanAngleRight { get; set; }
    public required decimal HighestGForce { get; set; }
}
