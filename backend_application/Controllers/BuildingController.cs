using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Controllers;

[Route("buildings")]
[ApiController]
public class BuildingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public BuildingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Building>> GetBuildings()
    {
        var buildings =  _context.Buildings.ToList();

        if (buildings.Any())
        {
            return Ok(buildings);
        }
        return NotFound();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Building>> GetBuilding(int id)
    {
        var building = await _context.Buildings.FindAsync(id);

        if (building == null)
        {
            return NotFound();
        }
        return Ok(building);
    }
    
    [HttpGet("{id:int}/users")]
    public async Task<ActionResult<IEnumerable<User>>> GetBuildingUsers(int id)
    {
        var building = await _context.Buildings
            .Include(b => b.Users)
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (building == null || !building.Users.Any())
        {
            return NotFound();
        }
        return Ok(building.Users.ToList());
    }
}    