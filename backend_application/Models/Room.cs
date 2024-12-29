using System.Text.Json.Serialization;

namespace API.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public int BuildingId { get; set; }
    
    [JsonIgnore]
    public Building Building { get; set; } = null!;

    public List<Device> Devices { get; set; } = [];
}