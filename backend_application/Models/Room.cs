using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend_application.Models;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    public int BuildingId { get; set; }
    
    [JsonIgnore]
    public Building Building { get; set; } = null!;

    public List<Device> Devices { get; set; } = [];
}