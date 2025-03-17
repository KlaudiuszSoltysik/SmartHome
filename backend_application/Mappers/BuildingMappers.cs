using backend_application.Data;
using backend_application.Models;
using backend_application.Dtos;
using Newtonsoft.Json.Linq;

namespace backend_application.Mappers;

public class BuildingMappers
{
    public static BuildingGetDto BuildBuildingGetDto(Building building)
    {
        List<string> users = new List<string>();
        
        foreach (var user in building.Users.ToList())
        {
            users.Add(user.Name);
        }
        return new BuildingGetDto 
            { 
            Id = building.Id,
            Name = building.Name,
            Address = building.Address,
            PostalCode = building.PostalCode,
            Country = building.Country,
            Rooms = building.Rooms,
            Users = users,
            };
    }

    public static async Task<Building> BuildBuildingPost(BuildingPostDto buildingDto, User user, ApplicationDbContext context)
    {
        var randomKey = CreateKey(context);
        var buildingDictionary = await ValidateAddress(buildingDto.Address, buildingDto.PostalCode, buildingDto.Country);
        
        return new Building
        {
            Key = randomKey,
            Name = buildingDto.Name,
            Address = buildingDictionary["address"],
            PostalCode = buildingDictionary["postalCode"],
            Country = buildingDictionary["country"],
            Latitude = Convert.ToDouble(buildingDictionary["lat"]),
            Longitude = Convert.ToDouble(buildingDictionary["lon"]),
            Users = new List<User> {user},
        };
    }
    
    private static string CreateKey(ApplicationDbContext context)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int length = 20;
        var random = new Random();
        var result = new char[length];
        string randomKey;
        
        do
        {
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            
            randomKey = new string(result);
        } while (context.Buildings.Any(b => b.Key == randomKey));

        return randomKey;
    }

    private static async Task<Dictionary<string, string>> ValidateAddress(string address, string postalCode, string country)
    {
        string url =
            $"https://nominatim.openstreetmap.org/search.php?street={address}&country={country}&postalcode={postalCode}&format=jsonv2".Replace(' ', '+');
        
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (compatible; AddressValidationApp)");
        var response = await client.GetStringAsync(url);

        if (response == null)
        {
            throw new ApplicationException("Address validation failed");
        }
        
        var jsonArray = JArray.Parse(response);
        var lat = jsonArray[0]["lat"]?.ToString();
        var lon = jsonArray[0]["lon"]?.ToString();
        
        url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json";
        response = await client.GetStringAsync(url);
        var jsonObject = JObject.Parse(response);
        
        return new Dictionary<string, string>
        {
            {"address", $"{jsonObject["address"]?["road"]} {jsonObject["address"]?["house_number"]}"},
            {"postalCode", jsonObject["address"]?["postcode"]?.ToString()},
            {"country", jsonObject["address"]?["country_code"]?.ToString()},
            {"lat", lat},
            {"lon", lon},
        };
    }
}