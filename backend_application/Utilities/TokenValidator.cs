using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend_application.Data;
using backend_application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend_application.Utilities;

public class TokenValidator
{
    private readonly IConfiguration _configuration;

    public TokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ClaimsPrincipal ValidateInvitationToken(string token)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = securityKey
            }, out var validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    public static async Task<User> GetUserFromToken(string token, ApplicationDbContext context)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        
        var claimsPrincipal = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var userIdFromToken = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdFromToken))
        {
            return null;
        }
        
        var expClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim != null && DateTime.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime)
        {
            return null;
        }
        
        var returnUser = await context.Users
            .Include(u => u.Buildings)
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userIdFromToken));
        
        return returnUser!;
    } 
}