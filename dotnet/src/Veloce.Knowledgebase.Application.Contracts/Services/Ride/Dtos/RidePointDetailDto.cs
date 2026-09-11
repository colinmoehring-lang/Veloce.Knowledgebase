namespace Veloce.Knowledgebase.Services.Ride;

public class RidePointDetailDto
{
    public required Guid RidePointId { get; set; } = Guid.NewGuid();
    public required decimal Speed { get; set; }
    public required decimal GForce { get; set; }
    public required decimal Lean { get; set; }
}
