namespace Veloce.Knowledgebase.Services.Vehicle;

public class UpdateVehicleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public required string VehicleType { get; set; }
    public byte[]? ImageData { get; set; }
}
