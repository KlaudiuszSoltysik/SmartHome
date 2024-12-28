namespace API.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public List<Device> Devices { get; set; } = [];
}