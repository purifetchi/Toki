namespace Toki.ActivityPub.Models;

/// <summary>
/// The base model.
/// </summary>
public abstract class AbstractModel
{
    /// <summary>
    /// The id of this object.
    /// </summary>
    public required Ulid Id { get; init; }
}