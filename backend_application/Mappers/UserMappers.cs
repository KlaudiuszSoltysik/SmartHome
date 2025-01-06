using System.Text.RegularExpressions;
using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Mappers;

public class UserMappers
{
    public static UserGetDtoShort BuildUserGetDtoShort(User user)
    {
        return new UserGetDtoShort{
            Name = user.Name,
        };
    }
    
    public static UserGetDtoFull BuildUserGetDtoFull(User user)
    {
        var buildings = new List<string>();
        foreach (var building in user.Buildings)
        {
            buildings.Add(building.Name);
        }
        
        return new UserGetDtoFull{
            Name = user.Name,
            Email = user.Email,
            Buildings = buildings,
        };
    }
    
    public static async Task<User> BuildUserPostRegister(UserPostRegisterDto userDto, ApplicationDbContext context)
    {
        if (userDto.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long");
        }
        if (!Regex.IsMatch(userDto.Password, @"[!@#$%^&*(),.?:{}|<>]"))
        {
            throw new ArgumentException("Password must contain at least one special character (!@#$%^&*(),.?:{}|<>)");
        }
        if (!Regex.IsMatch(userDto.Password, @"[A-Z]"))
        {
            throw new ArgumentException("Password must contain at least one uppercase letter");
        }
        if (!Regex.IsMatch(userDto.Password, @"[a-z]"))
        {
            throw new ArgumentException("Password must contain at least one lowercase letter");
        }
        if (!Regex.IsMatch(userDto.Password, @"[0-9]"))
        {
            throw new ArgumentException("Password must contain at least one number");
        }
        if (await context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email) != null)
        {
            throw new ArgumentException("User already exists");
        }
        

        var passwordHasher = new PasswordHasher<User>();
        
        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
        };
        
        user.Password = passwordHasher.HashPassword(user, userDto.Password);

        return user;
    }
}