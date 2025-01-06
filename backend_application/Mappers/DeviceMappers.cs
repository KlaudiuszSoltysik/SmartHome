using backend_application.Models;
using Azure.Core;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class DeviceMappers
{
    public static DeviceGetDto BuildDeviceGetDto(Device device)
    {
        return new DeviceGetDto
        {
            Id = device.Id,
            Name = device.Name,
            Type = device.Type,
            DeviceData = device.DeviceData
                .OrderByDescending(d => d.DateTime)
                .FirstOrDefault(),
        };
    }
    
    public static Device BuildDevicePostDto(Room room, DevicePostDto deviceDto)
    {
        return new Device
        {
            Name = deviceDto.Name,
            Type = deviceDto.Type,
            RoomId = room.Id,
            Room = room
        };
    }
}