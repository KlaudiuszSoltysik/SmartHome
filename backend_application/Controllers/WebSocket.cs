using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using backend_application.Data;
using System.Collections.Concurrent;
using System.Text;

namespace backend_application.Controllers;

public class FrameStorageService
{
    public readonly ConcurrentDictionary<int, byte[]> frames = new();
    
    public void AddFrame(byte[] frameData)
    {
        frames[0] = frameData;
    }

    public byte[] GetFrame()
    {
        return frames[0];
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

        while (webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 32];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receiveResult.MessageType == WebSocketMessageType.Binary)
            {
                byte[] frameData = buffer.Take(receiveResult.Count).ToArray();
                _frameStorageService.AddFrame(frameData); 
            }
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
    
    [Route("/receive")]
    public async Task ReceiveFrames()
    {
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        
            try
            {
                var buffer = new byte[1024 * 32];
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonMessage);
                    if (data != null && data.TryGetValue("token", out JsonElement tokenElement))
                    {
                        string token = tokenElement.GetString();
                    }
                }

                while (webSocket.State == WebSocketState.Open)
                {
                    byte[] frameData = _frameStorageService.GetFrame();
                    await webSocket.SendAsync(new ArraySegment<byte>(frameData), WebSocketMessageType.Binary, true,
                        CancellationToken.None);

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
        

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }

    // [Route("/receive")]
    // public async Task ReceiveFrames()
    // {
    //     using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
    //
    //     while (webSocket.State == WebSocketState.Open)
    //     {
    //         try
    //         {
    //             var buffer = new byte[1024 * 64];
    //             var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    //             
    //             if (receiveResult.MessageType == WebSocketMessageType.Text)
    //             {
    //                 string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
    //                 var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonMessage);
    //                 if (data != null && data.TryGetValue("token", out JsonElement tokenElement))
    //                 {
    //                     string token = tokenElement.GetString();
    //                 }
    //             }
    //             byte[] frameData = _frameStorageService.GetFrame();
    //             await webSocket.SendAsync(new ArraySegment<byte>(frameData), WebSocketMessageType.Binary, true, CancellationToken.None);
    //
    //             await Task.Delay(100);
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"WebSocket error: {ex.Message}");
    //         }
    //     }
    //
    //     await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    // }
}
