namespace Veloce.Knowledgebase.Services.Vehicle;

public class VehicleDto
{
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public byte[]? ImageData { get; set; }
}
