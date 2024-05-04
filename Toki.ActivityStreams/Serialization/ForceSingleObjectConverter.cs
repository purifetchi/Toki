using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Serialization;

/// <summary>
/// A converter that forces arrays into a single object..
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
public class ForceSingleObjectConverter<T> : JsonConverter<T>
{
    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
            
            // Most servers put the highest quality thing at the end, so we'll select the end.
            return list is not null ? 
                list[^1] :
                default;
        }

        var obj = JsonSerializer.Deserialize<T>(ref reader, options);
        return obj;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}