namespace API.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Building> Buildings { get; set; } = [];
}