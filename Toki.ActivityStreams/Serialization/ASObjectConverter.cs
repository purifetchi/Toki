using System.Text.Json;
using System.Text.Json.Serialization;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Activities.Extensions;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Serialization;

/// <summary>
/// The converter for a generic ASObject.
/// </summary>
public class ASObjectConverter : JsonConverter<ASObject>
{
    public override ASObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Check the type of the current token
        
        // If we have a string, this is a link to another ASObject.
        if (reader.TokenType == JsonTokenType.String)
        {
            return new ASObject()
            {
                Id = reader.GetString()!
            };
        }

        // If we have a start object, we're parsing the actual ASObject here.
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var obj = JsonDocument.ParseValue(ref reader);
            if (!obj.RootElement.TryGetProperty("type", out var typeProp))
                throw new JsonException("ASObject has no type!");

            return typeProp.GetString()! switch
            {
                "Create" => obj.Deserialize<ASCreate>(options: options),
                "Follow" => obj.Deserialize<ASFollow>(options: options),
                "Accept" => obj.Deserialize<ASAccept>(options: options),
                "Like" => obj.Deserialize<ASLike>(options: options),
                "Announce" => obj.Deserialize<ASAnnounce>(options: options),
                "Undo" => obj.Deserialize<ASUndo>(options: options),
                "Update" => obj.Deserialize<ASUpdate>(options: options),
                "Delete" => obj.Deserialize<ASDelete>(options: options),
                "Reject" => obj.Deserialize<ASReject>(options: options),
                "Add" => obj.Deserialize<ASAdd>(options: options),
                "Remove" => obj.Deserialize<ASRemove>(options: options),
                "Bite" => obj.Deserialize<ASBite>(options: options),
                
                "Note" => obj.Deserialize<ASNote>(options: options),
                "Video" => obj.Deserialize<ASVideo>(options: options),
                
                "Person" or "Service" or "Organization" or "Group" or "Application" => obj.Deserialize<ASActor>(options: options),
                
                "Collection" => obj.Deserialize<ASCollection<ASObject>>(options: options),
                "OrderedCollection" => obj.Deserialize<ASOrderedCollection<ASObject>>(options: options),
                
                "CollectionPage" => obj.Deserialize<ASCollectionPage<ASObject>>(options: options),
                "OrderedCollectionPage" => obj.Deserialize<ASOrderedCollectionPage<ASObject>>(options: options),

                // We don't understand this object yet.
                _ => new ASObject()
                {
                    Type = typeProp.GetString()!,
                    Id = ""
                }
            };
        }

        throw new JsonException("Unknown definition for ASObject parsing!");
    }

    public override void Write(Utf8JsonWriter writer, ASObject value, JsonSerializerOptions options)
    {
        if (!value.IsResolved)
        {
            writer.WriteStringValue(value.Id);
            return;
        }

        var obj = JsonSerializer.SerializeToElement(value, value.GetType());
        obj.WriteTo(writer);
    }
}