using System.Text.Json;
using System.Text.Json.Serialization;
using Toki.ActivityStreams.Context.Entries;

namespace Toki.ActivityStreams.Context.Serialization;

/// <summary>
/// A converter for the JSON-LD context object.
/// </summary>
public class LdContextConverter : JsonConverter<LdContext>
{
    /// <inheritdoc />
    public override LdContext? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.String => new LdContext().AddLink(reader.GetString()!),
        JsonTokenType.StartArray => ReadEntries(ref reader),
        JsonTokenType.Null => new LdContext(),
        _ => throw new JsonException("Invalid type for '@context'!")
    };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, LdContext value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        WriteEntries(
            writer,
            value.Remote);
        
        writer.WriteStartObject();
        
        WriteEntries(
            writer,
            value.Local);
        
        writer.WriteEndObject();

        writer.WriteEndArray();
    }

    /// <summary>
    /// Reads the entries.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The resulting LD context.</returns>
    private LdContext ReadEntries(
        ref Utf8JsonReader reader)
    {
        var context = new LdContext();

        while (reader.Read() && 
               reader.TokenType != JsonTokenType.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    context.AddLink(reader.GetString()!);
                    continue;
                
                case JsonTokenType.StartObject:
                    var obj = JsonDocument.ParseValue(ref reader);
                    var elems = ReadLocalSection(obj.RootElement);
                    context.AddMany(elems);
                    continue;
                
                default:
                    throw new JsonException($"Invalid type inside of '@context'! {reader.TokenType}");
            }
        }
        
        return context;
    }

    /// <summary>
    /// Reads the local section of the LD context.
    /// </summary>
    /// <param name="element">The JSON element.</param>
    /// <returns>The list of entries within it.</returns>
    private List<ILdContextEntry> ReadLocalSection(
        JsonElement element)
    {
        var list = new List<ILdContextEntry>();

        foreach (var child in element.EnumerateObject())
        {
            ILdContextEntry? value = child.Value.ValueKind switch
            {
                JsonValueKind.String => new KeyValueLdContextEntry(child.Name, child.Value.GetString()!),
                JsonValueKind.Object => new KeyObjectLdContextEntry(child.Name, ReadLocalSection(child.Value)),
                JsonValueKind.Null => null, // TODO: No idea how to actually handle this for now.
                _ => throw new JsonException($"Invalid type inside of '@context' local section! {child.Value.ValueKind}")
            };
            
            if (value is not null)
                list.Add(value);
        }
        
        return list;
    }

    /// <summary>
    /// Writes the entries.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="entries">The LD context entries.</param>
    private void WriteEntries(
        Utf8JsonWriter writer,
        IEnumerable<ILdContextEntry> entries)
    {
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case LinkLdContextEntry link:
                    writer.WriteStringValue(link.Link);
                    break;
                case KeyValueLdContextEntry keyValue:
                    writer.WritePropertyName(keyValue.Key);
                    writer.WriteStringValue(keyValue.Value);
                    break;
                
                case KeyObjectLdContextEntry keyObject:
                    writer.WritePropertyName(keyObject.Key);
                    writer.WriteStartObject();
                    WriteEntries(
                        writer,
                        keyObject.Entries);
                    writer.WriteEndObject();
                    break;
            }
        }
    }
}