using System.ComponentModel.DataAnnotations;

namespace backend_application.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(64)]
    [MinLength(64)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    public List<Building> Buildings { get; set; } = [];
}