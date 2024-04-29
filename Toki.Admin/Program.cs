// See https://aka.ms/new-console-template for more information

using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Toki.ActivityPub;
using Toki.ActivityPub.Configuration;
using Toki.Admin.Commands.AddUser;
using Toki.Admin.Commands.Setup;
using Toki.Admin.Configuration;

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

builder.Services.AddSingleton<IOptions<AdminConfiguration>>(_ => Options.Create(new AdminConfiguration
{
    AppSettingsPath = appsettings
}));

builder.Services.AddActivityPubServices();
builder.Services.AddTransient<AddUserHandler>()
    .AddTransient<SetupHandler>();

var host = builder.Build();

await Parser.Default.ParseArguments<AddUserOptions, SetupOptions>(args)
    .MapResult(
        (AddUserOptions opts) => host.Services.GetRequiredService<AddUserHandler>().Handle(opts),
        (SetupOptions opts) => host.Services.GetRequiredService<SetupHandler>().Handle(opts),
        errs => Task.CompletedTask);