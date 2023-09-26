using System.Net.WebSockets;
using System.Text;
using server.Utils;

namespace server.Models;

public class WebSocketConnection
{
    private readonly byte[] _buffer = new byte[1024 * 4];
    private readonly List<byte> _payload = new(1024 * 4);

    private WebSocket WebSocket { get; }
    public Guid Id { get; }
    public bool IsConnectionAlive => !WebSocket.CloseStatus.HasValue;

    public WebSocketConnection(WebSocket webSocket) : this(webSocket, Guid.NewGuid())
    {

    }

    public WebSocketConnection(WebSocket webSocket, Guid id)
        => (WebSocket, Id) = (webSocket, id);

    public async Task<bool> SendAsync(string message, CancellationToken cancellationToken)
    {
        if (WebSocket.State is WebSocketState.Aborted or WebSocketState.Closed) return false;

        var bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(bytes);
        await WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);

        return true;
    }

    public async Task<bool> SendAsync<T>(T message, CancellationToken cancellationToken)
    {
        var serialized = JsonUtils.Serialize<T>(message);
        return await SendAsync(serialized, cancellationToken);
    }

    public async Task<(WebSocketMessageType, string)> ReceiveAsync(CancellationToken cancellationToken)
    {
        _payload.Clear();

        try
        {
            WebSocketReceiveResult? result;

            do
            {
                result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), cancellationToken);
                _payload.AddRange(new ArraySegment<byte>(_buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await CloseAsync(cancellationToken);
            }

            return (result.MessageType, Encoding.UTF8.GetString(_payload.ToArray()));
        }
        catch (OperationCanceledException)
        {
            return (WebSocketMessageType.Close, string.Empty);
        }
    }

    public async Task<bool> CloseAsync(CancellationToken cancellationToken)
    {
        if (WebSocket.State is WebSocketState.Aborted or WebSocketState.Closed) return false;
        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
        return true;
    }
}