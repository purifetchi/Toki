namespace Toki.Admin.Configuration;

/// <summary>
/// The admin configuration.
/// </summary>
public class AdminConfiguration
{
    /// <summary>
    /// The path to the AppSettings file.
    /// </summary>
    public required string AppSettingsPath { get; init; }
}