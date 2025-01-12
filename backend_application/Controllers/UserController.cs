using backend_application.Data;
using backend_application.Dtos;
using backend_application.Models;
using backend_application.Mappers;
using backend_application.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenGenerator _tokenGenerator;
    
    public UserController(ApplicationDbContext context, JwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }
    
    [HttpGet]
    public async Task<ActionResult<User>> GetLoggedUser()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Authorization token is missing.");
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var user = await GetUserIdFromTokenClass.GetUserIdFromToken(token, _context);
            
            var userDto = UserMappers.BuildUserGetDtoFull(user);
            return Ok(userDto);
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserGetDtoFull>> PostRegister([FromBody] UserPostRegisterDto userDto)
    {
        try
        {
            var userModel = await UserMappers.BuildUserPostRegister(userDto, _context);
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<UserGetDtoFull>> PostLogin([FromBody] UserPostLoginDto userDto)
    {
        var user = await _context.Users
            .Include(u => u.Buildings)
            .FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (user == null)
        {
            return NotFound("Email address not found.");
        }
        
        var passwordHasher = new PasswordHasher<User>();
        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, userDto.Password);
        if (verificationResult != PasswordVerificationResult.Success)
        {
            return Unauthorized("Invalid password.");
        }
        
        var token = _tokenGenerator.GenerateToken(user.Id);
        return Ok(new Dictionary<string, dynamic>
        {
            {"user", UserMappers.BuildUserGetDtoFull(user)},
            {"token", token}
        });
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