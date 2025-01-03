using backend_application.Models;
using Azure.Core;
using backend_application.Dtos;

namespace backend_application.Mappers;

public class DeviceMappers
{
    public static DeviceDto BuildDeviceDto(Device device)
    {
        return new DeviceDto
        {
            Name = device.Name,
            Type = device.Type,
        };
    }
}