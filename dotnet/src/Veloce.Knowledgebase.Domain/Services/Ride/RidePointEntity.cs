namespace Veloce.Knowledgebase.Services.Ride;

public class RidePointEntity
{
    public required Guid RidePointId { get; set; } = Guid.NewGuid();
    public required Guid RideId { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string Coordinate { get; set; }
    public required decimal Speed { get; set; }
    public required decimal GForce { get; set; }
    public required decimal Lean { get; set; }
}
