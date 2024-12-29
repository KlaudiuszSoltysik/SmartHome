using System.Text.Json.Serialization;

namespace API.Models;

public class DeviceData
{
    public int Id { get; set; }
    public string Data { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
    
    public int DeviceId { get; set; }
    
    [JsonIgnore]
    public Device Device { get; set; } = null!;
}