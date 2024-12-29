using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings/{buildingId}/rooms/{roomId}/devices")]
[ApiController]
public class DeviceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public DeviceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices(int roomId)
    {
        var room = await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null || !room.Devices.Any())
        {
            return NotFound();
        }
        return Ok(room.Devices.ToList());
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Device>> GetDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound();
        }
        return Ok(device);
    }
}