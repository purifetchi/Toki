namespace Toki.ActivityStreams.Context.Entries;

/// <summary>
/// A key-value context entry.
/// </summary>
/// <param name="Key">The key.</param>
/// <param name="Value">The value.</param>
public record KeyValueLdContextEntry(string Key, string Value) : ILdContextEntry;