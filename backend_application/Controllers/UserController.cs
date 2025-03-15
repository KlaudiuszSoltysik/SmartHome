﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IEmailSender _emailSender;
    private readonly TokenValidator _tokenValidator;
    
    public UserController(ApplicationDbContext context, JwtTokenGenerator tokenGenerator, IEmailSender emailSender, TokenValidator tokenValidator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _emailSender = emailSender;
        _tokenValidator = tokenValidator;
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
            var user = await TokenValidator.GetUserFromToken(token, _context);
            
            var userDto = UserMappers.BuildUserGetDtoFull(user);
            return Ok(userDto);
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(int userId, string code)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            if (user.ConfirmationCode != code)
            {
                return BadRequest("Invalid confirmation code.");
            }
            
            user.IsActive = true;
            user.ConfirmationCode = null;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Redirect("http://localhost:5173/login");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserGetDtoFull>> PostRegister([FromBody] UserPostRegisterDto userDto)
    {
        try
        {
            var userModel = await UserMappers.BuildUserPostRegister(userDto, _context);
            
            var confirmationCode = Guid.NewGuid().ToString();
            
            userModel.ConfirmationCode = confirmationCode;
            
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userModel.Email);

            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "User",
                values: new { userId = user.Id, code = confirmationCode },
                protocol: HttpContext.Request.Scheme
            );
            
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Confirm your email <a href='{callbackUrl}'>here</a>.");

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
        
        if (!user.IsActive)
        {
            return Unauthorized("Account is not activated.");
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
    
    [HttpGet("refresh")]
    public async Task<ActionResult> RefreshToken()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Authorization token is missing.");
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var user = await TokenValidator.GetUserFromToken(token, _context);
            
            var newToken = _tokenGenerator.GenerateToken(user.Id);
            return Ok(new Dictionary<string, dynamic>
            { {"token", newToken} });
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            var resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpires = DateTime.UtcNow.AddMinutes(15);
   
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            var frontendUrl = "http://localhost:5173/reset-password";
            var resetLink = $"{frontendUrl}?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
            
            var emailBody = $@"
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <p>If you didn't request this, please ignore this email.</p>";
            
            await _emailSender.SendEmailAsync(user.Email, "Reset Password", emailBody);
    
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto userDto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            if (user.ResetPasswordToken != userDto.Token || DateTime.UtcNow > user.ResetPasswordExpires)
            {
                return BadRequest("Invalid or expired reset token.");
            }
            
            ValidatePasswordClass.ValidatePassword(userDto.Password);
        
            var passwordHasher = new PasswordHasher<User>();
        
            user.Password = passwordHasher.HashPassword(user, userDto.Password);
            
            user.ResetPasswordToken = null;
            user.ResetPasswordExpires = null;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
    
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] AddUserDto addUserDto)
    {
        var buildingId = addUserDto.BuildingId;
        var email = addUserDto.Email;
        
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Authorization token is missing.");
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var user = await TokenValidator.GetUserFromToken(token, _context);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Buildings.All(b => b.Id != buildingId))
            {
                return BadRequest("Building not found.");
            }

            if (_context.Users.All(u => u.Email != email))
            {
                var subject = "Invitation to create account";
                var body = $"Create free account at <a href='http://localhost:5173/register'>SmartHome</a>";
            
                await _emailSender.SendEmailAsync(email, subject, body);
            }
            
            var invitationToken = _tokenGenerator.GenerateInvitationToken(email, buildingId);

            var invitationLink = Url.Action(
                action: "AcceptInvitation",
                controller: "User",
                values: new { token = invitationToken },
                protocol: HttpContext.Request.Scheme
            );
                
            var subject2 = "Invitation to Join Building";
            var body2 = $"You have been invited to join a building. Click the following link to accept the invitation: <a href='{invitationLink}'>Accept Invitation</a>";
            
            await _emailSender.SendEmailAsync(email, subject2, body2);
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }

        return Ok();
    }
    
    [HttpGet("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        var principal = _tokenValidator.ValidateInvitationToken(token);

        if (principal == null)
        {
            return Unauthorized("Invalid or expired token.");
        }
        
        var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                    ?? principal.FindFirst("email")?.Value;
        var buildingId = int.Parse(principal.FindFirst("buildingId")?.Value);
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var building = await _context.Buildings
            .Include(b => b.Users)
            .FirstOrDefaultAsync(b => b.Id == buildingId);

        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        if (!building.Users.Contains(user))
        {
            building.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return Ok("User successfully added to the building.");
    }
    
    [HttpPut]
    public async Task<ActionResult<UserGetDtoFull>> Put([FromBody] UserPutDto userDto)
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Authorization token is missing.");
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var user = await TokenValidator.GetUserFromToken(token, _context);
            
            user.Name = userDto.Name;
        
            await _context.SaveChangesAsync();
        
            return Ok(UserMappers.BuildUserGetDtoFull(user));
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Authorization token is missing.");
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var user = await TokenValidator.GetUserFromToken(token, _context);
            
            foreach (var building in user.Buildings.ToList())
            {
                var userCountInBuilding = await _context.BuildingUser
                    .CountAsync(bu => bu.BuildingId == building.Id);

                if (userCountInBuilding == 1)
                {
                    _context.Buildings.Remove(building);
                }
            }
        
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
    
            return Ok();
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }
}