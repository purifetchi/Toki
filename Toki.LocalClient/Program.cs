using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toki.ActivityPub;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;
using System.CommandLine;
using Toki.ActivityPub.Persistence.Repositories;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(svc =>
        svc.AddActivityPubServices())
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
root.Add(user);

await root.InvokeAsync(args);