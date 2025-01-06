using System.ComponentModel.DataAnnotations;
using backend_application.Models;

namespace backend_application.Dtos;

public class RoomGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Device> Devices { get; set; }
}

public class RoomPostDto
{
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
}