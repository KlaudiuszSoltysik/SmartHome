using API.Data;
using API.Models;
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
    public async Task<ActionResult<IEnumerable<Building>>> GetRooms(int buildingId)
    {
        var building = await _context.Buildings
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.Id == buildingId);
        
        if (building == null || !building.Rooms.Any())
        {
            return NotFound();
        }
        return Ok(building.Rooms.ToList());
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Building>> GetRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound();
        }
        return Ok(room);
    }
}