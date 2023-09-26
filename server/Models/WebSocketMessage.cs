using Newtonsoft.Json;

namespace server.Models;

public enum WebSocketClientEvent
{
    LocationUpdate
}

public enum WebSocketServerEvent
{
    LocationUpdate
}

public record WebSocketClientMessage(
    [property: JsonProperty("event")] WebSocketClientEvent Event);


public record WebSocketServerMessage(
    [property: JsonProperty("event")] WebSocketServerEvent Event);

public record Position(
    [property: JsonProperty("x")] int X,
    [property: JsonProperty("y")] int Y);

public record ClientLocationUpdateEvent(
    [property: JsonProperty("position")] Position Position)
    : WebSocketClientMessage(WebSocketClientEvent.LocationUpdate);

public record ServerLocationUpdateEvent(
    [property: JsonProperty("position")] Position Position,
    [property: JsonProperty("id")] Guid Guid)
    : WebSocketServerMessage(WebSocketServerEvent.LocationUpdate);