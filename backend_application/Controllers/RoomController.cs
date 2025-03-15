using backend_application.Data;
using backend_application.Dtos;
using backend_application.Mappers;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomGetDto>>> GetAll(int buildingId)
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
            
            var rooms = user.Buildings.FirstOrDefault(b => b.Id == buildingId).Rooms;
    
            if (rooms.Any())
            {
                var roomDtos = new List<RoomGetDto>();
                foreach (var room in rooms)
                {
                    roomDtos.Add(RoomMappers.BuildRoomGetDto(room));
                }
                return Ok(roomDtos);
            }
            return Ok();
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomGetDto>> GetById(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        var roomDto = RoomMappers.BuildRoomGetDto(room);
        return Ok(roomDto);
    }
    
    [HttpPost]
    public async Task<ActionResult<RoomGetDto>> Post(int buildingId, [FromBody] RoomPostDto roomDto)
    {
        var building = await _context.Buildings.FindAsync(buildingId);
        
        if (building == null)
        {
            return NotFound("Building not found.");
        }
        
        var roomModel = RoomMappers.BuildRoomPostDto(building, roomDto);
        await _context.Rooms.AddAsync(roomModel); await _context.SaveChangesAsync();
        var roomGetDto = RoomMappers.BuildRoomGetDto(roomModel);
        return CreatedAtAction(nameof(GetById), new {  buildingId, roomModel.Id }, roomGetDto);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomGetDto>> Put(int id, [FromBody] RoomPostDto roomDto)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        room.Name = roomDto.Name;
        
        await _context.SaveChangesAsync();
        
        return Ok(RoomMappers.BuildRoomGetDto(room));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return Ok();
    }
}