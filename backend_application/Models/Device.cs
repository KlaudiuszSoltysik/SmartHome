using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend_application.Models;

public class Device
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Type { get; set; } = string.Empty;
    
    public int RoomId { get; set; }
    
    [JsonIgnore]
    public Room Room { get; set; } = null!;
    
    public List<User> AcceptedUsers { get; set; } = [];
}