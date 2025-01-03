namespace backend_application.Dtos;

public class DeviceDataDto
{
    public string Data { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
}