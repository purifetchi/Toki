using CommandLine;

namespace Toki.Admin.Commands.AddUser;

/// <summary>
/// Adds a user.
/// </summary>
[Verb("adduser", HelpText = "Add a user.")]
public class AddUserOptions
{
    /// <summary>
    /// The username of the user.
    /// </summary>
    [Option('u', "username", HelpText = "The username of the user.", Required = true)]
    public required string Username { get; init; }
    
    /// <summary>
    /// The password of the user.
    /// </summary>
    [Option('p', "password", HelpText = "The password of the user.", Required = true)]
    public required string Password { get; init; }
}
