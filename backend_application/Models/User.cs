using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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
    
    [MaxLength(256)]
    [MinLength(256)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = false;
    
    [MaxLength(36)]
    public string? ConfirmationCode { get; set; } = string.Empty;
    
    [MaxLength(72)]
    public string? ResetPasswordToken { get; set; } = string.Empty;
    
    public DateTime? ResetPasswordExpires { get; set; } = DateTime.Now;
    
    public List<Building> Buildings { get; set; } = [];
}