namespace Toki.ActivityStreams.Context.Entries;

/// <summary>
/// A key-object LD context entry.
/// </summary>
/// <param name="Key">The key.</param>
/// <param name="Entries">The values within it.</param>
public record KeyObjectLdContextEntry(string Key, List<ILdContextEntry> Entries) : ILdContextEntry;