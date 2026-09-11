namespace Veloce.Knowledgebase.Services.Ride;

public class StartRideDto
{
    public required string VehicleType { get; set; }
    public Guid? VehicleId { get; set; }
}
