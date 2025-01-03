using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using backend_application.Mappers;
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
    public ActionResult<IEnumerable<BuildingGetDto>> GetAll()
    {
        var buildings =  _context.Buildings.ToList();

        if (buildings.Any())
        {
            var buildingsDtos = new List<BuildingGetDto>();
            foreach (var building in buildings)
            {
                buildingsDtos.Add(BuildingMappers.BuildBuildingGetDto(building));
            }
            return Ok(buildingsDtos);
        }
        return NotFound();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BuildingGetDto>> GetById(int id)
    {
        var building = await _context.Buildings.FindAsync(id);

        if (building == null)
        {
            return NotFound();
        }

        var buildingDto = BuildingMappers.BuildBuildingGetDto(building);
        
        return Ok(buildingDto);
    }
    
    [HttpGet("{id:int}/users")]
    public async Task<ActionResult<IEnumerable<UserDtoShort>>> GetRelatedUsers(int id)
    {
        var building = await _context.Buildings
            .Include(b => b.Users)
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (building == null || !building.Users.Any())
        {
            return NotFound();
        }
        
        var users = building.Users.ToList();
        var userDto = new List<UserDtoShort>();
        foreach (var user in users)
        {
            userDto.Add(UserMappers.BuildUserDtoShort(user));
        }
        return Ok(userDto);
    }

    [HttpPost]
    public async Task<ActionResult<BuildingGetDto>> Post([FromBody] BuildingPostDto buildingDto)
    {
        try
        {
            var buildingModel = await BuildingMappers.BuildBuildingPost(buildingDto, _context);
            await _context.Buildings.AddAsync(buildingModel);
            await _context.SaveChangesAsync();
            var buildingGetDto = BuildingMappers.BuildBuildingGetDto(buildingModel);
            return CreatedAtAction(nameof(GetById), new { id = buildingModel.Id }, buildingGetDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BuildingGetDto>> Put([FromRoute] int id, [FromBody] BuildingPutDto buildingDto)
    {
        var building = await _context.Buildings.FindAsync(id);

        if (building == null)
        {
            return NotFound();
        }
        
        var buildingDictionary = await BuildingMappers.ValidateAddress(buildingDto.Address, buildingDto.PostalCode, buildingDto.Country);
        
        building.Name = buildingDto.Name;
        building.Address = buildingDictionary["address"];
        building.PostalCode = buildingDictionary["postalCode"];
        building.Country = buildingDictionary["country"];
        building.Latitude = Convert.ToDouble(buildingDictionary["lat"]);
        building.Longitude = Convert.ToDouble(buildingDictionary["lon"]);
        
        await _context.SaveChangesAsync();
        
        return BuildingMappers.BuildBuildingGetDto(building);
    }
}    