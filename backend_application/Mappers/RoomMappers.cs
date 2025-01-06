using backend_application.Models;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class RoomMappers
{
    public static RoomGetDto BuildRoomGetDto(Room room)
    {
        return new RoomGetDto
        {
            Id = room.Id,
            Name = room.Name,
            Devices = room.Devices,
        };
    }
    
    public static Room BuildRoomPostDto(Building building, RoomPostDto roomDto)
    {
        return new Room
        {
            Name = roomDto.Name,
            BuildingId = building.Id,
            Building = building
        };
    }
}