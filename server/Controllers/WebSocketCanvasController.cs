using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using server.Models;
using server.Utils;

namespace server.Controllers;

[Route("ws/canvas")]
public class WebSocketCanvasController : ControllerBase
{
    private static readonly List<WebSocketConnection> Connections = new();
    private CancellationToken CancellationToken { get; }

    public WebSocketCanvasController(IHostApplicationLifetime lifetime)
    {
        CancellationToken = lifetime.ApplicationStopping;
    }

    [HttpGet]
    public async Task HandleConnection()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest is false)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var currentConnection = new WebSocketConnection(webSocket);     
        Connections.Add(currentConnection);

        try
        {
            do
            {
                var (messageType, raw) = await currentConnection.ReceiveAsync(CancellationToken);

                if (messageType == WebSocketMessageType.Close) return;

                var baseMessage = JsonUtils.Deserialize<WebSocketClientMessage>(raw);
                if (baseMessage is null) continue;

                if (baseMessage.Event is WebSocketClientEvent.LocationUpdate)
                {
                    var message = JsonUtils.Deserialize<ClientLocationUpdateEvent>(raw);

                    if (message is null) continue;

                    var tasks = new List<Task>();
                    Connections.ForEach(connection =>
                    {
                        if(connection.Id == currentConnection.Id) return;
                        tasks.Add(connection.SendAsync(new ServerLocationUpdateEvent(message.Position, currentConnection.Id), CancellationToken));
                    });

                    await Task.WhenAll(tasks.ToArray());
                }
            } while (currentConnection.IsConnectionAlive);
        }
        finally
        {
            await currentConnection.CloseAsync(CancellationToken);
            Connections.Remove(currentConnection);
        }
    }
}