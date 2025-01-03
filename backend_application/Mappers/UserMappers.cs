using backend_application.Models;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class UserMappers
{
    public static UserDtoShort BuildUserDtoShort(User user)
    {
        return new UserDtoShort{
            Name = user.Name,
        };
    }
    
    public static UserDtoFull BuildUserDtoFull(User user)
    {
        return new UserDtoFull{
            Name = user.Name,
            Email = user.Email,
        };
    }
}