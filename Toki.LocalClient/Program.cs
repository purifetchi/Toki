using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toki.ActivityPub;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(svc =>
        svc.AddActivityPubServices())
    .Build();
    
var resolver = builder.Services.GetRequiredService<ActivityPubResolver>();

// Service test
var test = await resolver.Fetch<ASActor>(ASObject.Link("https://miku.place/users/prefetcher"));
Console.WriteLine(test);