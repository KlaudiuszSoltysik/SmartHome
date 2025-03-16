using backend_application.Data;
using backend_application.Dtos;
using backend_application.Mappers;
using backend_application.Models;
using backend_application.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings")]
[ApiController]
public class BuildingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public BuildingController(ApplicationDbContext context)
    {
        _context = context;
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

        if (user == null)
        {
            return (NotFound("User not found."), null);
        }

        return (null, user); 
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BuildingGetDto>>> GetAll()
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        var buildings = await _context.Buildings
            .Where(b => b.Users.Any(u => u.Id == user.Id))
            .ToListAsync();

        if (buildings.Any())
        {
            var buildingDtos = new List<BuildingGetDto>();
            foreach (var building in buildings)
            {
                buildingDtos.Add(BuildingMappers.BuildBuildingGetDto(building));
            }
            return Ok(buildingDtos);
        }
        
        return NotFound("Buildings not found.");
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BuildingGetDto>> GetById(int id)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var building = await _context.Buildings
            .Where(b => b.Users.Any(u => u.Id == user.Id) && b.Id == id).FirstOrDefaultAsync();

        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        var buildingDto = BuildingMappers.BuildBuildingGetDto(building);
            
        return Ok(buildingDto);
    }
    
    [HttpGet("{id:int}/users")]
    public async Task<ActionResult<IEnumerable<UserGetDtoShort>>> GetRelatedUsers(int id)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        var building = await _context.Buildings
            .Include(b => b.Users)
            .Where(b => b.Users.Any(u => u.Id == user.Id) && b.Id == id).FirstOrDefaultAsync();
        
        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        var users = building.Users.ToList();
        var userDto = new List<UserGetDtoShort>();
        
        foreach (var u in users)
        {
            userDto.Add(UserMappers.BuildUserGetDtoShort(u));
        }
        
        return Ok(userDto);
    }

    [HttpPost]
    public async Task<ActionResult<BuildingGetDto>> Post([FromBody] BuildingPostDto buildingDto)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
            
        var buildingModel = await BuildingMappers.BuildBuildingPost(buildingDto, user, _context);
        
        await _context.Buildings.AddAsync(buildingModel);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BuildingGetDto>> Put(int id, [FromBody] BuildingPutDto buildingDto)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var building = await _context.Buildings
            .Where(b => b.Users.Any(u => u.Id == user.Id) && b.Id == id).FirstOrDefaultAsync();

        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        building.Name = buildingDto.Name;
        
        await _context.SaveChangesAsync();
        
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var building = await _context.Buildings
            .Where(b => b.Users.Any(u => u.Id == user.Id) && b.Id == id).FirstOrDefaultAsync();

        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        _context.Buildings.Remove(building);
        await _context.SaveChangesAsync();

        return Ok();
    }
}    