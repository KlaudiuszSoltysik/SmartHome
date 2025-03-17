using backend_application.Models;
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
            AcceptedUsers = device.AcceptedUsers,
        };
    }
    
    public static Device BuildDevicePostDto(Room room, DevicePostDto deviceDto)
    {
        return new Device
        {
            Name = deviceDto.Name,
            Type = deviceDto.Type,
            AcceptedUsers = deviceDto.AcceptedUsers,
            RoomId = room.Id,
            Room = room
        };
    }
}