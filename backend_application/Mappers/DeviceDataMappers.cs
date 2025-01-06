using backend_application.Models;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class DeviceDataMappers
{
    public static DeviceDataGetDto BuildDeviceDataGetDto(DeviceData deviceData)
    {
        return new DeviceDataGetDto
        {
            Data = deviceData.Data,
            DateTime = deviceData.DateTime,
        };
    }
}