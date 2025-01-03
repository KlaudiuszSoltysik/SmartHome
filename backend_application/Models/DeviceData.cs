using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend_application.Models;

public class DeviceData
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(250)]
    [MinLength(1)]
    public string Data { get; set; } = String.Empty;
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime DateTime { get; set; } = DateTime.Now;
    
    public int DeviceId { get; set; }
    
    [JsonIgnore]
    public Device Device { get; set; } = null!;
}