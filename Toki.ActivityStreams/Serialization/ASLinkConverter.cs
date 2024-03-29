using System.Text.Json;
using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Serialization;

/// <summary>
/// Converter for the <see cref="ASLink"/> base class.
/// </summary>
public class ASLinkConverter : JsonConverter<ASLink>
{
    /// <inheritdoc/>
    public override ASLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Tried to parse ASLink item that didn't start with a StartObject!");

        var obj = JsonDocument.ParseValue(ref reader);
        if (!obj.RootElement.TryGetProperty("type", out var typeProp))
            throw new JsonException("ASLink has no type!");

        return typeProp.GetString()! switch
        {
            "Image" => obj.Deserialize<ASImage>(),
            "Document" => obj.Deserialize<ASDocument>(),
            "Mention" => obj.Deserialize<ASMention>(),
            
            _ => new ASLink()
            {
                Type = typeProp.GetString()!
            }
        };
    }
    
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ASLink value, JsonSerializerOptions options)
    {
        var obj = JsonSerializer.SerializeToElement(value, value.GetType());
        obj.WriteTo(writer);
    }
}