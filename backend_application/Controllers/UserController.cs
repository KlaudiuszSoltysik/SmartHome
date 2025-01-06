using backend_application.Data;
using backend_application.Dtos;
using backend_application.Models;
using backend_application.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _context.Users
            .Include(u => u.Buildings)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        if (user == null)
        {
            return NotFound();
        }
        
        var userDto = UserMappers.BuildUserGetDtoFull(user);
        return Ok(userDto);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserGetDtoFull>> Post([FromBody] UserPostRegisterDto userDto)
    {
        try
        {
            var userModel = await UserMappers.BuildUserPostRegister(userDto, _context);
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync();
            var userGetDto = UserMappers.BuildUserGetDtoFull(userModel);
            return CreatedAtAction(nameof(GetById), new { userModel.Id }, userGetDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<UserGetDtoFull>> Post([FromBody] UserPostLoginDto userDto)
    {
        var user = await _context.Users
            .Include(u => u.Buildings)
            .FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (user == null)
        {
            return NotFound();
        }
        
        var passwordHasher = new PasswordHasher<User>();
        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, userDto.Password);
        if (verificationResult != PasswordVerificationResult.Success)
        {
            return ValidationProblem();
        }
        
        // IMPLEMENT LOGIN
        
        return Ok(UserMappers.BuildUserGetDtoFull(user));
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserGetDtoFull>> Put(int id, [FromBody] UserPutDto userDto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        
        user.Name = userDto.Name;
        
        await _context.SaveChangesAsync();
        
        return Ok(UserMappers.BuildUserGetDtoFull(user));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = await _context.Users
            .Include(u => u.Buildings)
            .FirstOrDefaultAsync(u => u.Id == id); 

        if (user == null)
        {
            return NotFound();
        }

        foreach (var building in user.Buildings.ToList())
        {
            if (building.Users.Count == 1)
            {
                _context.Buildings.Remove(building);
            }
        }
        
        _context.Users.Remove(user);
        // IMPLEMENT LOGOUT
        await _context.SaveChangesAsync();

        return Ok();
    }
}