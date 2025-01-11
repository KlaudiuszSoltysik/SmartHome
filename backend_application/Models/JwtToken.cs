using System.ComponentModel.DataAnnotations;

namespace backend_application.Models;

public class JwtToken
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Token { get; set; }
    
    [Required]
    public DateTime InvalidatedAt { get; set; }
}