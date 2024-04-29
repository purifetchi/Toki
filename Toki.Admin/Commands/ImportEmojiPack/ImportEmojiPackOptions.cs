using CommandLine;

namespace Toki.Admin.Commands.ImportEmojiPack;

[Verb("importemojipack", HelpText = "Import an emoji pack.")]
public class ImportEmojiPackOptions
{
    /// <summary>
    /// The path to the file.
    /// </summary>
    [Option('p', "path", HelpText = "The path to the file.", Required = true)]
    public required string Path { get; init; }
}