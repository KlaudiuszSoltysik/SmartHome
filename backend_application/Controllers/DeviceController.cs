using backend_application.Data;
using backend_application.Dtos;
using backend_application.Mappers;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceGetDto>>> GetAll(int roomId)
    {
        var room = await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null || !room.Devices.Any())
        {
            return NotFound("No devices found.");
        }

        var devices = room.Devices.ToList();
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
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound("Device not found.");
        }
        
        var deviceDto = DeviceMappers.BuildDeviceGetDto(device);
        return Ok(deviceDto);
    }
    
    [HttpPost]
    public async Task<ActionResult<DeviceGetDto>> Post(int buildingId, int roomId, [FromBody] DevicePostDto deviceDto)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        
        if (room == null)
        {
            return NotFound("Room not found.");
        }
        
        var deviceModel = DeviceMappers.BuildDevicePostDto(room, deviceDto);
        await _context.Devices.AddAsync(deviceModel); 
        await _context.SaveChangesAsync();
        var deviceGetDto = DeviceMappers.BuildDeviceGetDto(deviceModel);
        return CreatedAtAction(nameof(GetById), new {  buildingId, roomId, deviceModel.Id }, deviceGetDto);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceGetDto>> Put(int id, [FromBody] DevicePutDto deviceDto)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound("Device not found.");
        }
        
        device.Name = deviceDto.Name;
        
        await _context.SaveChangesAsync();
        
        return Ok(DeviceMappers.BuildDeviceGetDto(device));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound("Device not found.");
        }
        
        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return Ok();
    }
}