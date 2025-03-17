using backend_application.Data;
using backend_application.Dtos;
using backend_application.Mappers;
using backend_application.Models;
using backend_application.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings/{buildingId:int}/rooms/{roomId:int}/devices")]
[ApiController]
public class DeviceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public DeviceController(ApplicationDbContext context)
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

        return (null, user); 
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceGetDto>>> GetAll(int roomId)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }
        
        var devices = await _context.Buildings
            .Where(b => b.Users.Contains(user))
            .SelectMany(b => b.Rooms)
            .Where(r => r.Id == roomId)
            .SelectMany(r => r.Devices)
            .ToListAsync();  
        
        if (devices.Count == 0)
        {
            return NotFound("No devices found.");
        }
        
        var deviceDtos = new List<DeviceGetDto>();
        
        foreach (var device in devices)
        {
            deviceDtos.Add(DeviceMappers.BuildDeviceGetDto(device));
        }
        
        return Ok(deviceDtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviceGetDto>> GetById(int id)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var device = await _context.Buildings
            .Where(b => b.Users.Contains(user))
            .SelectMany(b => b.Rooms)
            .SelectMany(r => r.Devices)
            .FirstOrDefaultAsync(d => d.Id == id);
        
        if (device == null)
        {
            return NotFound("Device not found.");
        }
        
        var deviceDto = DeviceMappers.BuildDeviceGetDto(device);
        
        return Ok(deviceDto);
    }
    
    [HttpPost]
    public async Task<ActionResult<DeviceGetDto>> Post(int roomId, [FromBody] DevicePostDto deviceDto)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var room = await _context.Buildings
            .Where(b => b.Users.Contains(user))
            .SelectMany(b => b.Rooms)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        var deviceModel = DeviceMappers.BuildDevicePostDto(room, deviceDto);
        
        await _context.Devices.AddAsync(deviceModel); 
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceGetDto>> Put(int id, [FromBody] DevicePutDto deviceDto)
    {
        var (errorResult, user) = await ValidateAndGetUser(Request);
    
        if (errorResult != null)
        {
            return errorResult;
        }

        var device = await _context.Buildings
            .Where(b => b.Users.Contains(user)) 
            .SelectMany(b => b.Rooms)
            .SelectMany(r => r.Devices)
            .FirstOrDefaultAsync(d => d.Id == id);
        
        if (device == null)
        {
            return NotFound("Device not found.");
        }
        
        device.Name = deviceDto.Name;
        
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

        var device = await _context.Buildings
            .Where(b => b.Users.Contains(user)) 
            .SelectMany(b => b.Rooms)
            .SelectMany(r => r.Devices)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
        {
            return NotFound("Device not found.");
        }
        
        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return Ok();
    }
}