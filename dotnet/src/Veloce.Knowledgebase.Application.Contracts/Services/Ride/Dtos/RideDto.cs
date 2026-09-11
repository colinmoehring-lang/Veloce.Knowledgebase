namespace Veloce.Knowledgebase.Services.Ride;

public class RideDto
{
    public Guid RideId { get; set; }
    public Guid UserId { get; set; }
    public Guid? VehicleId { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Duration { get; set; }
    public decimal Distance { get; set; }
    public decimal HighestSpeed { get; set; }
    public string CoordinateHighestSpeed { get; set; } = string.Empty;
    public decimal AverageSpeed { get; set; }
    public decimal HighestLeanAngleLeft { get; set; }
    public string CoordinateHighestLeanAngleLeft { get; set; } = string.Empty;
    public decimal HighestLeanAngleRight { get; set; }
    public string CoordinateHighestLeanAngleRight { get; set; } = string.Empty;
    public decimal HighestGForce { get; set; }
}
