using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toki.ActivityPub;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.WebFinger;
using Toki.ActivityStreams.Objects;
using Toki.HTTPSignatures;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(svc =>
        svc.AddActivityPubServices()
            .AddLogging()
            .AddHttpSignatures())
    .Build();
    
var root = new RootCommand();

var user = new Command("user", "User-specific commands");
var userCreate = new Command("create", "Create a user.");
var username = new Option<string>(
    name: "--username",
    description: "The username of the user");
var password = new Option<string>(
    name: "--password",
    description: "The password of the user.");

userCreate.Add(username);
userCreate.Add(password);
userCreate.SetHandler(async (usernameValue, passwordValue) =>
{
    var svc = builder.Services.GetRequiredService<UserRepository>();
    var result = await svc.CreateNewUser(usernameValue, passwordValue);
    Console.WriteLine(result is null ? "Creating user failed." : $"Id: {result.Id}");
}, username, password);

user.Add(userCreate);

var userImport = new Command("import", "Import a user.");
var handle = new Option<string>(
    name: "--handle",
    description: "The handle of the user");

userImport.Add(handle);
userImport.SetHandler(async handleValue =>
{
    var svc = builder.Services.GetRequiredService<UserRepository>();
    var wf = builder.Services.GetRequiredService<WebFingerResolver>();
    var fetch = builder.Services.GetRequiredService<ActivityPubResolver>();

    var resp = await wf.FingerAtHandle(handleValue);
    var id = resp?.Links?
        .FirstOrDefault(l => l.Type == "application/activity+json")?
        .Hyperlink;

    if (id is null)
        return;

    var actor = await fetch.Fetch<ASActor>(ASObject.Link(id));
    if (actor is null)
        return;

    await svc.ImportFromActivityStreams(actor);
}, handle);

user.Add(userImport);
root.Add(user);

await root.InvokeAsync(args);