using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using backend_application.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings/{buildingId}/rooms")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public RoomController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll(int buildingId)
    {
        var building = await _context.Buildings
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.Id == buildingId);
        
        if (building == null || !building.Rooms.Any())
        {
            return NotFound();
        }

        var rooms = building.Rooms.ToList();
        var roomsDtos = new List<RoomDto>();
        foreach (var room in rooms)
        {
            roomsDtos.Add(RoomMappers.BuildRoomDto(room));
        }
        return Ok(roomsDtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound();
        }
        
        var roomDto = RoomMappers.BuildRoomDto(room);
        return Ok(roomDto);
    }
}