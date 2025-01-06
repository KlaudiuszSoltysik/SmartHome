using System.ComponentModel.DataAnnotations;

namespace backend_application.Dtos;

public class DeviceDataGetDto
{
    public string Data { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
}