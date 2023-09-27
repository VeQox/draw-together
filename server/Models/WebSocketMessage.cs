using Newtonsoft.Json;

namespace server.Models;

public enum WebSocketClientEvent
{
    UpdateLocation
}

public enum WebSocketServerEvent
{
    UpdateLocation
}

public record WebSocketClientMessage(
    [property: JsonProperty("event")] WebSocketClientEvent Event);


public record WebSocketServerMessage(
    [property: JsonProperty("event")] WebSocketServerEvent Event);

public record Position(
    [property: JsonProperty("x")] int X,
    [property: JsonProperty("y")] int Y);

public record ClientUpdateLocationMessage(
    [property: JsonProperty("position")] Position Position)
    : WebSocketClientMessage(WebSocketClientEvent.UpdateLocation);

public record ServerUpdateLocationMessage(
    [property: JsonProperty("position")] Position Position,
    [property: JsonProperty("id")] Guid Guid)
    : WebSocketServerMessage(WebSocketServerEvent.UpdateLocation);