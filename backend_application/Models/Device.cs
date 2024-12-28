namespace API.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Type { get; set; }
    
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    
    public List<DeviceData> DeviceData { get; set; } = [];
}