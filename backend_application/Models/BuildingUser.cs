namespace backend_application.Models;

public class BuildingUser
{
    public int BuildingId { get; set; }
    public Building Building { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
}
