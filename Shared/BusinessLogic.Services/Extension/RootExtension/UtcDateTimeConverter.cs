using System.Text.Json;
using System.Text.Json.Serialization;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var dt = DateTime.Parse(reader.GetString()!);
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();

        writer.WriteStringValue(
            utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
        );
    }
}
