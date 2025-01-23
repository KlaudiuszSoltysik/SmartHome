using System.ComponentModel.DataAnnotations;
using backend_application.Models;

namespace backend_application.Dtos;

public class UserGetDtoShort
{
    public string Name { get; set; } = string.Empty;
}

public class UserGetDtoFull
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Buildings { get; set; } = [];
}

public class UserPostRegisterDto
{
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class UserPostLoginDto
{
    [Required]
    [MaxLength(50)]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class UserResetPasswordDto
{
    [Required]
    [MaxLength(50)]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    public string Email { get; set; }
    
    [MaxLength(72)]
    [MinLength(18)]
    public string Token { get; set; }
    
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}


public class UserPutDto
{
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
}