using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings/{buildingId}/rooms/{roomId}/devices/{deviceId}/devicedata")]
[ApiController]
public class DeviceDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public DeviceDataController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<Device>> GetDeviceData(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceData)
            .FirstOrDefaultAsync(b => b.Id == deviceId);
        
        if (device == null || !device.DeviceData.Any())
        {
            return NotFound();
        }
        return Ok(device.DeviceData.ToList());
    }
    
    [HttpGet("last")]
    public async Task<ActionResult<Device>> GetLastDeviceData(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceData)
            .FirstOrDefaultAsync(d => d.Id == deviceId);
        
        if (device == null || !device.DeviceData.Any())
        {
            return NotFound();
        }
        return Ok(device.DeviceData
            .OrderByDescending(d => d.DateTime)
            .FirstOrDefault());
    }
}