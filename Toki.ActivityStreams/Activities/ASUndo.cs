namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams undo activity.
/// </summary>
public class ASUndo : ASActivity
{
    /// <summary>
    /// Constructs a new undo activity.
    /// </summary>
    public ASUndo()
        : base("Undo")
    {
        
    }
}