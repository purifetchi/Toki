using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toki.Serialization;

/// <summary>
/// A JSON converter for the Zulu timestamp (YYYY-MM-DDTHH:MM:SSZ)
/// </summary>
public class ZuluTimestampConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // NOTE: We aren't going to be reading in timestamps like this.
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("s") + ".000Z");
    }
}