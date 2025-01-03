using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using backend_application.Mappers;
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
    public async Task<ActionResult<DeviceDataDto>> GetAll(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceData)
            .FirstOrDefaultAsync(b => b.Id == deviceId);
        
        if (device == null || !device.DeviceData.Any())
        {
            return NotFound();
        }
        
        var deviceData = device.DeviceData.ToList();
        var deviceDataDtos = new List<DeviceDataDto>();
        foreach (var deviceDataSingular in deviceData)
        {
            deviceDataDtos.Add(DeviceDataMappers.BuildDeviceDataDto(deviceDataSingular));
        }
        return Ok(deviceDataDtos);
    }
    
    [HttpGet("last")]
    public async Task<ActionResult<DeviceDataDto>> GetLast(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceData)
            .FirstOrDefaultAsync(d => d.Id == deviceId);
        
        if (device == null || !device.DeviceData.Any())
        {
            return NotFound();
        }

        var deviceData = device.DeviceData
            .OrderByDescending(d => d.DateTime)
            .FirstOrDefault();
        
        var deviceDataDto = DeviceDataMappers.BuildDeviceDataDto(deviceData);
        return Ok(deviceDataDto);
    }
}