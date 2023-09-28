using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace server.Utils;

public static class JsonUtils
{
    public static string Serialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public static T? Deserialize<T>(string json)
    {
        var settings = new JsonSerializerSettings
        {
            Error = (_, args) => args.ErrorContext.Handled = true
        };

        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static bool TryDeserialize<T>(string json, [NotNullWhen(true)]out T? deserialized) {
        deserialized = Deserialize<T>(json);
        return deserialized is not null;
    }
}