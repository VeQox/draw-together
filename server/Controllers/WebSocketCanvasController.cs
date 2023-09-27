using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
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
        var connection = new WebSocketConnection(webSocket);

        Connections.Add(connection);

        try
        {
            do
            {
                var (messageType, serializedMessage) = await connection.ReceiveAsync(CancellationToken);
                if (messageType == WebSocketMessageType.Close) return;

                await HandleMessage(serializedMessage, connection);
            } while (connection.IsConnectionAlive);
        }
        finally
        {
            await connection.CloseAsync(CancellationToken);
            Connections.Remove(connection);
        }
    }

    private async Task HandleMessage(string serializedMessage, WebSocketConnection currentConnection) {
        if(!JsonUtils.TryGetDeserialized(serializedMessage, out WebSocketClientMessage? baseMessage)) return;

        if (baseMessage.Event is WebSocketClientEvent.UpdateLocation)
        {
            if(!JsonUtils.TryGetDeserialized(serializedMessage, out ClientUpdateLocationMessage? message)) return;

            await HandleUpdateLocationMessage(message, currentConnection);
        }
    }

    private async Task HandleUpdateLocationMessage(ClientUpdateLocationMessage message, WebSocketConnection currentConnection) {
        var tasks = new List<Task>();
        var serverUpdateLocationMessage = new ServerUpdateLocationMessage(message.Position, currentConnection.Id);

        foreach (var connection in Connections)
        {
            if (connection.Id == currentConnection.Id) continue;
            tasks.Add(connection.SendAsync(serverUpdateLocationMessage, CancellationToken));
        }

        await Task.WhenAll(tasks.ToArray());
    }
}