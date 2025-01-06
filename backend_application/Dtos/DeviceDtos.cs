using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using backend_application.Models;

namespace backend_application.Dtos;

public class DeviceGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    
    [AllowNull]
    public DeviceData DeviceData { get; set; }
}

public class DevicePostDto
{
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Type { get; set; } = string.Empty;
}

public class DevicePutDto
{
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
}
