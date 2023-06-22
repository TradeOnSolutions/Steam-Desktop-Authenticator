using Newtonsoft.Json;

namespace SteamAuthentication.Trades.Responses.Converters;

public class ParseStringToULongConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(ulong) || t == typeof(ulong?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);

        if (ulong.TryParse(value, out var l))
            return l;

        throw new Exception("Cannot unmarshal type long");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (ulong)untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}