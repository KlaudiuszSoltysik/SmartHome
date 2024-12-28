using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend_application.Controllers;

[Route("building")]
[ApiController]
public class BuildingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public BuildingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Building>>> GetBuildings()
    {
        var buildings =  _context.Buildings.ToList();
        
        return Ok(buildings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Building>> GetBuilding(int id)
    {
        var building = await _context.Buildings.FindAsync(id);

        if (building == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(building);
        }
    }
}    