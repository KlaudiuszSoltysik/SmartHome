using backend_application.Data;
using backend_application.Dtos;
using backend_application.Mappers;
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
        var building = await _context.Buildings
            .Include(b => b.Rooms)
            .ThenInclude(r => r.Devices)
            .FirstOrDefaultAsync(b => b.Id == buildingId);
        
        if (building == null || !building.Rooms.Any())
        {
            return NotFound();
        }

        var rooms = building.Rooms.ToList();
        var roomsDtos = new List<RoomGetDto>();
        foreach (var room in rooms)
        {
            roomsDtos.Add(RoomMappers.BuildRoomGetDto(room));
        }
        return Ok(roomsDtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomGetDto>> GetById(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            return NotFound();
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
            return NotFound();
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
            return NotFound();
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
            return NotFound();
        }
        
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return Ok();
    }
}