using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend_application.Data;
using backend_application.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Utilities;

public class GetUserFromTokenClass
{
    public static async Task<User> GetUserFromToken(string token, ApplicationDbContext context)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new EvaluateException("No token provided.");
        }
        
        var claimsPrincipal = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var userIdFromToken = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdFromToken))
        {
            throw new EvaluateException("Token is invalid.");
        }
        
        var expClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim != null && DateTime.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime)
        {
            throw new EvaluateException("Token is expired.");
        }
        
        var returnUser = await context.Users
            .Include(u => u.Buildings)
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userIdFromToken));
        
        return returnUser!;
    }
}