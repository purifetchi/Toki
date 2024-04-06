using System.Text.Json.Serialization;
using Toki.ActivityStreams.Context.Entries;
using Toki.ActivityStreams.Context.Serialization;

namespace Toki.ActivityStreams.Context;

/// <summary>
/// The JSON-LD context of an object.
/// </summary>
[JsonConverter(typeof(LdContextConverter))]
public class LdContext
{
    /// <summary>
    /// The default used LD context.
    /// </summary>
    public static LdContext Default { get; } = new LdContext()
        .AddLink("https://www.w3.org/ns/activitystreams");
    
    /// <summary>
    /// The remote entries.
    /// </summary>
    public IEnumerable<ILdContextEntry> Remote => _remote;

    /// <summary>
    /// The local entries.
    /// </summary>
    public IEnumerable<ILdContextEntry> Local => _local;
    
    /// <summary>
    /// The remote LD context entries.
    /// </summary>
    private readonly List<ILdContextEntry> _remote = [];

    /// <summary>
    /// The local LD context entries.
    /// </summary>
    private readonly List<ILdContextEntry> _local = [];

    /// <summary>
    /// Adds a link entry to the context.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns>This context.</returns>
    public LdContext AddLink(string link)
    {
        _remote.Add(
            new LinkLdContextEntry(link));
        
        return this;
    }

    /// <summary>
    /// Adds a key/value entry to the context.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>This context.</returns>
    public LdContext AddKeyValue(string key, string value)
    {
        _local.Add(
            new KeyValueLdContextEntry(key, value));
        return this;
    }
    
    /// <summary>
    /// Adds a key/object entry to the context.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="entries">The entries for the object.</param>
    /// <returns>This context.</returns>
    public LdContext AddKeyObject(string key, List<ILdContextEntry> entries)
    {
        _local.Add(
            new KeyObjectLdContextEntry(key, entries));
        return this;
    }

    /// <summary>
    /// Adds many entries to this context.
    /// </summary>
    /// <param name="entries">The entries.</param>
    /// <returns>This context.</returns>
    public LdContext AddMany(IEnumerable<ILdContextEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry is LinkLdContextEntry)
                _remote.Add(entry);
            else
                _local.Add(entry);
        }

        return this;
    }
}