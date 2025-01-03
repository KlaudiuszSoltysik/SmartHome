using backend_application.Models;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class DeviceDataMappers
{
    public static DeviceDataDto BuildDeviceDataDto(DeviceData deviceData)
    {
        return new DeviceDataDto
        {
            Data = deviceData.Data,
            DateTime = deviceData.DateTime,
        };
    }
}