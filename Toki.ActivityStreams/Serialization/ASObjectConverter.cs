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
                "Create" => obj.Deserialize<ASCreate>(),
                "Follow" => obj.Deserialize<ASFollow>(),
                "Accept" => obj.Deserialize<ASAccept>(),
                "Like" => obj.Deserialize<ASLike>(),
                "Announce" => obj.Deserialize<ASAnnounce>(),
                "Undo" => obj.Deserialize<ASUndo>(),
                "Update" => obj.Deserialize<ASUpdate>(),
                "Delete" => obj.Deserialize<ASDelete>(),
                "Reject" => obj.Deserialize<ASReject>(),
                "Add" => obj.Deserialize<ASAdd>(),
                "Remove" => obj.Deserialize<ASRemove>(),
                "Bite" => obj.Deserialize<ASBite>(),
                
                "Note" => obj.Deserialize<ASNote>(),
                "Video" => obj.Deserialize<ASVideo>(),
                
                "Person" or "Service" or "Organization" or "Group" or "Application" => obj.Deserialize<ASActor>(),
                
                "Collection" => obj.Deserialize<ASCollection<ASObject>>(),
                "OrderedCollection" => obj.Deserialize<ASOrderedCollection<ASObject>>(),
                
                "CollectionPage" => obj.Deserialize<ASCollectionPage<ASObject>>(),
                "OrderedCollectionPage" => obj.Deserialize<ASOrderedCollectionPage<ASObject>>(),

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