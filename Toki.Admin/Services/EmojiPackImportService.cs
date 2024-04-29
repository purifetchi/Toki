using System.IO.Compression;
using Toki.ActivityPub.Emojis;
using Toki.Services.Drive;

namespace Toki.Admin.Services;

/// <summary>
/// Helper class for importing emoji packs. 
/// </summary>
public class EmojiPackImportService(
    EmojiService emojiSvc,
    DriveService driveSvc)
{
    /// <summary>
    /// Imports an emoji pack from a zip file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    public async Task ImportFromZip(
        string file)
    {
        await using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var zipArchive = new ZipArchive(fs);

        foreach (var entry in zipArchive.Entries)
        {
            var data = new byte[entry.Length];
            
            await using var stream = entry.Open();
            stream.ReadExactly(data);
            
            using var sr = new BinaryReader(stream);
            var name = entry.Name.Split('.')[0];
            
            Console.WriteLine($"Importing {name}");
            var url = driveSvc.StoreFromBytes(
                entry.Name,
                data);

            if (url is null)
            {
                await Console.Error.WriteLineAsync($"Failed to store {name}.");
                continue;
            }

            var emoji = await emojiSvc.CreateLocalEmoji(
                name,
                url);

            if (emoji is null)
                await Console.Error.WriteLineAsync($"Failed to create {name}.");
        }
    }
}