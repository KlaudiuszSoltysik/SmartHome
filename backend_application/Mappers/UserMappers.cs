using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using backend_application.Utilities;
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
        ValidatePasswordClass.ValidatePassword(userDto.Password);
        
        if (await context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email) != null)
        {
            throw new ArgumentException("User already exists.");
        }
        
        var passwordHasher = new PasswordHasher<User>();
        
        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
        };
        
        user.Password = passwordHasher.HashPassword(user, userDto.Password);
        user.IsActive = false;

        return user;
    }
}