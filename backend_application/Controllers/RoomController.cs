using backend_application.Data;
using backend_application.Dtos;
using backend_application.Mappers;
using backend_application.Models;
using backend_application.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings/{buildingId:int}/rooms")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public RoomController(ApplicationDbContext context)
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
    public async Task<ActionResult<IEnumerable<RoomGetDto>>> GetAll(int buildingId)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
            
        var rooms = await _context.Buildings
            .Include(b=>b.Rooms)
            .Where(b => b.Id == buildingId)
            .Where(b=>b.Users.Contains(user))
            .SelectMany(b=>b.Rooms)
            .ToListAsync();

        if (rooms.Count == 0)
        {
            return NotFound("No rooms found.");
        }
        
        var roomDtos = new List<RoomGetDto>();
        
        foreach (var room in rooms)
        {
            roomDtos.Add(RoomMappers.BuildRoomGetDto(room));
        }
        
        return Ok(roomDtos);
        }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomGetDto>> GetById(int id)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var room = await _context.Buildings
            .Include(b => b.Rooms)
            .Where(b => b.Users.Contains(user))
            .SelectMany(b => b.Rooms)
            .FirstOrDefaultAsync(r=>r.Id == id);
        
        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        var buildingDto = RoomMappers.BuildRoomGetDto(room);
            
        return Ok(buildingDto);
    }
    
    [HttpPost]
    public async Task<ActionResult<RoomGetDto>> Post(int buildingId, [FromBody] RoomPostDto roomDto)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var building = await _context.Buildings
            .Include(b => b.Rooms)
            .Where(b => b.Users.Contains(user))
            .FirstOrDefaultAsync(b => b.Id == buildingId);
        
        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        var roomModel = RoomMappers.BuildRoomPostDto(building, roomDto);
        
        await _context.Rooms.AddAsync(roomModel); 
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomGetDto>> Put(int id, [FromBody] RoomPostDto roomDto)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var room = await _context.Buildings
            .Include(b => b.Rooms)
            .Where(b => b.Users.Contains(user))
            .SelectMany(b => b.Rooms)
            .FirstOrDefaultAsync(r=>r.Id == id);

        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        room.Name = roomDto.Name;
        
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

        var room = await _context.Buildings
            .Include(b => b.Rooms)
            .Where(b => b.Users.Contains(user))
            .SelectMany(b => b.Rooms)
            .FirstOrDefaultAsync(r=>r.Id == id);

        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return Ok();
    }
}