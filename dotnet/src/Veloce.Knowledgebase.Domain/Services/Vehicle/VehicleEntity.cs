namespace Veloce.Knowledgebase.Services.Vehicle;

public class VehicleEntity
{
    public required Guid VehicleId { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public required string VehicleType { get; set; }
    public byte[]? ImageData { get; set; }
}
