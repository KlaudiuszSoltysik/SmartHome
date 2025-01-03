﻿using System.ComponentModel.DataAnnotations;

namespace backend_application.Dtos;

public class BuildingGetDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class BuildingPostDto
{
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
}

public class BuildingPutDto
{
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
}