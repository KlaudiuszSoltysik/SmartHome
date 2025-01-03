using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using backend_application.Mappers;
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
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll(int roomId)
    {
        var room = await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null || !room.Devices.Any())
        {
            return NotFound();
        }

        var devices = room.Devices.ToList();
        var deviceDtos = new List<DeviceDto>();
        foreach (var device in devices)
        {
            deviceDtos.Add(DeviceMappers.BuildDeviceDto(device));
        }
        return Ok(deviceDtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviceDto>> GetById(int id)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound();
        }
        
        var deviceDto = DeviceMappers.BuildDeviceDto(device);
        return Ok(deviceDto);
    }
}