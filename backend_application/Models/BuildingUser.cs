using System.ComponentModel.DataAnnotations;

namespace backend_application.Models;

public class BuildingUser
{
    [Required]
    public int BuildingId { get; set; }
    [Required]
    public Building Building { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public User User { get; set; }
}
