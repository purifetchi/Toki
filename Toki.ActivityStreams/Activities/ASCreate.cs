using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams create activity.
/// </summary>
public class ASCreate : ASActivity
{
    /// <summary>
    /// Constructs a new ASCreate activity.
    /// </summary>
    public ASCreate()
        : base("Create")
    {
        
    }
}