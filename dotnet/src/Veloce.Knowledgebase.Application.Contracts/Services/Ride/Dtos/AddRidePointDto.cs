namespace Veloce.Knowledgebase.Services.Ride;

public class AddRidePointDto
{
    public required DateTime Timestamp { get; set; }
    public required string Coordinate { get; set; }
    public required decimal Speed { get; set; }
    public required decimal GForce { get; set; }
    public decimal Lean { get; set; } = 0;
}
