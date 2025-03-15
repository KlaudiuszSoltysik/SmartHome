using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using backend_application.Data;
using System.Collections.Concurrent;
using System.Text;
using backend_application.Utilities;

namespace backend_application.Controllers;

public class FrameStorageService
{
    public ConcurrentDictionary<string, ConcurrentDictionary<string, dynamic>> frames = new();

    public void AddFrame(int buildingId, int roomId, int cameraId, byte[] frame)
    {
        string key = $"{buildingId}_{roomId}_{cameraId}";

        var frameData = frames.GetOrAdd(key, _ => new ConcurrentDictionary<string, dynamic>());
        frameData["building"] = buildingId;
        frameData["room"] = roomId;
        frameData["camera"] = cameraId;
        frameData["frame"] = frame;
    }

    public byte[] GetFrame(int buildingId, int roomId, int cameraId)
    {
        string key = $"{buildingId}_{roomId}_{cameraId}";

        return frames.TryGetValue(key, out var frameData) && frameData.TryGetValue("frame", out var frame)
            ? frame as byte[]
            : null;
    }
}

public class WebSocketController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly FrameStorageService _frameStorageService;

    public WebSocketController(ApplicationDbContext context, FrameStorageService frameStorageService)
    {
        _context = context;
        _frameStorageService = frameStorageService;
    }
    
    [Route("/send")]
    public async Task SendFrames()
    {
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var buffer = new byte[1024];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (receiveResult.MessageType != WebSocketMessageType.Text)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Invalid message", CancellationToken.None);
            return;
        }

        string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonMessage);

        if (data == null || 
            !data.TryGetValue("token", out var tokenElement) ||
            !data.TryGetValue("building_id", out var buildingIdElement) ||
            !data.TryGetValue("room_id", out var roomIdElement) ||
            !data.TryGetValue("camera_id", out var cameraIdElement))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Missing parameters", CancellationToken.None);
            return;
        }
        string token = tokenElement.GetString();
        int buildingId = buildingIdElement.GetInt32();
        int roomId = roomIdElement.GetInt32();
        int cameraId = cameraIdElement.GetInt32();
        
        var user = await TokenValidator.GetUserFromToken(token, _context);
        
        if (user == null || user.Buildings.All(b => b.Id != buildingId))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Missing parameters", CancellationToken.None);
            return;
        }

        while (webSocket.State == WebSocketState.Open)
        {
            buffer = new byte[500000];
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receiveResult.MessageType == WebSocketMessageType.Binary)
            {
                byte[] frameData = buffer[..receiveResult.Count];
                _frameStorageService.AddFrame(buildingId, roomId, cameraId, frameData);
            }
            else
            {
                break;
            }
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
    
    [Route("/receive")]
    public async Task ReceiveFrames()
    {
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var buffer = new byte[1024];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (receiveResult.MessageType != WebSocketMessageType.Text)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Invalid message", CancellationToken.None);
            return;
        }

        string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonMessage);

        if (data == null || 
            !data.TryGetValue("token", out var tokenElement) ||
            !data.TryGetValue("building_id", out var buildingIdElement) ||
            !data.TryGetValue("room_id", out var roomIdElement) ||
            !data.TryGetValue("camera_id", out var cameraIdElement))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Missing parameters", CancellationToken.None);
            return;
        }
        
        string token = tokenElement.GetString();
        int buildingId = buildingIdElement.GetInt32();
        int roomId = roomIdElement.GetInt32();
        int cameraId = cameraIdElement.GetInt32();
        
        var user = await TokenValidator.GetUserFromToken(token, _context);
        
        if (user == null || user.Buildings.All(b => b.Id != buildingId))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Missing parameters", CancellationToken.None);
            return;
        }

        while (webSocket.State == WebSocketState.Open)
        {
            byte[] frameData = _frameStorageService.GetFrame(buildingId, roomId, cameraId);
            await webSocket.SendAsync(new ArraySegment<byte>(frameData), WebSocketMessageType.Binary, true, CancellationToken.None);
            
            await Task.Delay(50);
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
}
