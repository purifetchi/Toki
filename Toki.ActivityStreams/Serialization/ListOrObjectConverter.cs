using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Serialization;

/// <summary>
/// A converter for an object that can either be a list or a single object.
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
public class ListOrObjectConverter<T> : JsonConverter<IReadOnlyList<T>>
{
    /// <inheritdoc />
    public override IReadOnlyList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
            return JsonSerializer.Deserialize<List<T>>(ref reader, options);

        var obj = JsonSerializer.Deserialize<T>(ref reader, options);
        return obj is null ? null : [obj];
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}