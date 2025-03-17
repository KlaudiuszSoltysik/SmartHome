using System.Security.Claims;
using backend_application.Data;
using backend_application.Dtos;
using backend_application.Models;
using backend_application.Mappers;
using backend_application.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
    
    private async Task<(ActionResult?, User?)> ValidateAndGetUser(HttpRequest request)
    {
        var authorizationHeader = request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return (Unauthorized("Authorization token is missing."), null);
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var user = await TokenValidator.GetUserFromToken(token, _context);

        return (null, user); 
    }

    private bool EncodeFace(List<string> images, int userId)
    {
        List<string> byteArrayStrings = images
            .Select(base64 => string.Join(",", Convert.FromBase64String(base64)))
            .ToList();
        
        System.IO.File.WriteAllLines(@$"C:\Users\klaud\Documents\development\SmartHome\{userId}.txt", byteArrayStrings);
        
        string pythonVenv = @"C:\Users\klaud\Documents\development\SmartHome\image_processing\.venv\Scripts\python.exe";
        string scriptPath = @"C:\Users\klaud\Documents\development\SmartHome\image_processing\encode_faces.py";
        
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonVenv,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        psi.ArgumentList.Add(scriptPath);
        psi.ArgumentList.Add(userId.ToString());

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            System.IO.File.Delete(@$"C:\Users\klaud\Documents\development\SmartHome\{userId}.txt");

            if (process.ExitCode != 0)
            {
                return false;
            }
            
            return true;
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<User>> GetLoggedUser()
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        var userDto = UserMappers.BuildUserGetDtoFull(user);
        return Ok(userDto);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserGetDtoFull>> PostRegister([FromBody] UserPostRegisterDto userDto)
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
    
    [HttpPost("scan-face")]
    public async Task<ActionResult> ScanFace([FromBody] List<string> images)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        bool success = EncodeFace(images, user.Id);

        return success ? Ok("Face encodings stored successfully.") : BadRequest("Failed to store face encodings.");
    }
    
    [HttpGet("refresh")]
    public async Task<ActionResult> RefreshToken()
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        var newToken = _tokenGenerator.GenerateToken(user.Id);
        
        return Ok(new Dictionary<string, dynamic> { {"token", newToken} });
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(int userId, string code)
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
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
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
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto userDto)
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
    
    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] AddUserDto addUserDto)
    {
        var buildingId = addUserDto.BuildingId;
        var email = addUserDto.Email;
        
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
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

        return Ok();
    }
    
    [HttpGet("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        var principal = _tokenValidator.ValidateInvitationToken(token);
        
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
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        user.Name = userDto.Name;
    
        await _context.SaveChangesAsync();
    
        return Ok(UserMappers.BuildUserGetDtoFull(user));
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
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
}