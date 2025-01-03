using backend_application.Models;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class RoomMappers
{
    public static RoomDto BuildRoomDto(Room room)
    {
        return new RoomDto{
            Name = room.Name,
        };
    }
}