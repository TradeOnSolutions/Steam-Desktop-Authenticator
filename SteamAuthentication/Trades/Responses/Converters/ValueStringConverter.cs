using Newtonsoft.Json;

namespace SteamAuthentication.Trades.Responses.Converters;

public class ValueStringConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
        writer.Flush();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        throw new MethodAccessException();
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}