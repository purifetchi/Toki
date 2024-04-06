namespace Toki.ActivityStreams.Context.Entries;

/// <summary>
/// A link context entry.
/// </summary>
/// <param name="Link">The link.</param>
public record LinkLdContextEntry(string Link) : ILdContextEntry;