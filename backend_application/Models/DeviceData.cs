namespace API.Models;

public class DeviceData
{
    public int Id { get; set; }
    public string Data { get; set; }
    
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}