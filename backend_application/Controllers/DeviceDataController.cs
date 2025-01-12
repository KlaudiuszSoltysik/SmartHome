using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using backend_application.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings/{buildingId:int}/rooms/{roomId:int}/devices/{deviceId:int}/devicedata")]
[ApiController]
public class DeviceDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public DeviceDataController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DeviceDataGetDto>> GetAll(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceData)
            .FirstOrDefaultAsync(b => b.Id == deviceId);
        
        if (device == null || !device.DeviceData.Any())
        {
            return NotFound("No device data found.");
        }
        
        var deviceData = device.DeviceData.ToList();
        var deviceDataDtos = new List<DeviceDataGetDto>();
        foreach (var deviceDataSingular in deviceData)
        {
            deviceDataDtos.Add(DeviceDataMappers.BuildDeviceDataGetDto(deviceDataSingular));
        }
        return Ok(deviceDataDtos);
    }
    
    // [HttpGet("last")]
    // public async Task<ActionResult<DeviceDataGetDto>> GetLast(int deviceId)
    // {
    //     var device = await _context.Devices
    //         .Include(d => d.DeviceData)
    //         .FirstOrDefaultAsync(d => d.Id == deviceId);
    //     
    //     if (device == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     var deviceData = device.DeviceData
    //         .OrderByDescending(d => d.DateTime)
    //         .FirstOrDefault();
    //
    //     if (deviceData == null)
    //     {
    //         return NotFound();
    //     }
    //     
    //     var deviceDataDto = DeviceDataMappers.BuildDeviceDataGetDto(deviceData);
    //     return Ok(deviceDataDto);
    // }
}