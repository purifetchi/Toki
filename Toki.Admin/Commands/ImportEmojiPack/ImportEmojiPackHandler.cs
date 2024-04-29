using Toki.Admin.Services;

namespace Toki.Admin.Commands.ImportEmojiPack;

public class ImportEmojiPackHandler(
    EmojiPackImportService svc)
{
    public async Task Handle(
        ImportEmojiPackOptions opts)
    {
        await svc.ImportFromZip(opts.Path);
    }
}