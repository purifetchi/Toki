// See https://aka.ms/new-console-template for more information

using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Toki.ActivityPub;
using Toki.ActivityPub.Configuration;
using Toki.Admin.Commands.AddUser;
using Toki.Admin.Commands.ImportEmojiPack;
using Toki.Admin.Commands.Setup;
using Toki.Admin.Configuration;
using Toki.Admin.Services;
using Toki.Configuration;
using Toki.Services.Drive;

string appsettings;
try
{
    appsettings = args.First();
    args = args[1..];
}
catch (InvalidOperationException)
{
    Console.Error.WriteLine("The first parameter should be the appsettings.json path.");
    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile(appsettings);
builder.Services.Configure<InstanceConfiguration>(
    builder.Configuration.GetSection("Instance"));

builder.Services.Configure<UploadConfiguration>(
    builder.Configuration.GetSection("Upload"));


builder.Services.AddSingleton<IOptions<AdminConfiguration>>(_ => Options.Create(new AdminConfiguration
{
    AppSettingsPath = appsettings
}));

builder.Services.AddActivityPubServices();
builder.Services.AddTransient<DriveService>();
builder.Services.AddTransient<EmojiPackImportService>();
builder.Services.AddTransient<AddUserHandler>()
    .AddTransient<SetupHandler>()
    .AddTransient<ImportEmojiPackHandler>();

var host = builder.Build();

var uploadConf = host.Services.GetRequiredService<IOptions<UploadConfiguration>>();
if (uploadConf.Value.UploadFolderPath?.StartsWith('.') == true)
{
    // Adjust the upload path.
    var root = Path.GetDirectoryName(appsettings)!;
    uploadConf.Value.UploadFolderPath = Path.GetFullPath(
        Path.Combine(root, uploadConf.Value.UploadFolderPath));
}

await Parser.Default.ParseArguments<AddUserOptions, SetupOptions, ImportEmojiPackOptions>(args)
    .MapResult(
        (AddUserOptions opts) => host.Services.GetRequiredService<AddUserHandler>().Handle(opts),
        (SetupOptions opts) => host.Services.GetRequiredService<SetupHandler>().Handle(opts),
        (ImportEmojiPackOptions opts) => host.Services.GetRequiredService<ImportEmojiPackHandler>().Handle(opts),
        errs => Task.CompletedTask);