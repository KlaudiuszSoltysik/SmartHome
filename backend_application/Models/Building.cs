using System.ComponentModel.DataAnnotations;

namespace backend_application.Models;

public class Building
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    [MinLength(5)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    [MinLength(6)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2)]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    
    public List<User> Users { get; set; } = [];
    
    public List<Room> Rooms { get; set; } = [];
}