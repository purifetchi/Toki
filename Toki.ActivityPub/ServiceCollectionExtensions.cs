using Microsoft.Extensions.DependencyInjection;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Formatters;
using Toki.ActivityPub.NodeInfo;
using Toki.ActivityPub.Notifications;
using Toki.ActivityPub.OAuth2;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Renderers;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityPub.WebFinger;

namespace Toki.ActivityPub;

/// <summary>
/// Extensions for the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the ActivityPub services.
    /// </summary>
    /// <param name="collection">The collection to add them to.</param>
    public static IServiceCollection AddActivityPubServices(this IServiceCollection collection)
    {
        collection.AddDbContext<TokiDatabaseContext>();
        collection.AddScoped<InstanceActorResolver>();
        collection.AddTransient<ContentFormatter>();

        collection.AddScoped<InstanceRepository>();

        collection.AddScoped<OAuthRepository>()
            .AddScoped<OAuthManagementService>();
        
        collection.AddScoped<PostRepository>()
            .AddScoped<PostRenderer>()
            .AddScoped<PostManagementService>();
        
        collection.AddScoped<NotificationRepository>()
            .AddScoped<NotificationService>();
        
        collection.AddScoped<UserRepository>()
            .AddScoped<UserRenderer>()
            .AddScoped<FollowRepository>()
            .AddScoped<UserRelationService>()
            .AddScoped<UserSessionService>()
            .AddScoped<UserManagementService>();

        collection.AddTransient<ActivityPubResolver>()
            .AddTransient<ActivityPubMessageValidationService>()
            .AddTransient<NodeInfoResolver>()
            .AddTransient<InstancePathRenderer>()
            .AddTransient<MessageFederationService>();
        
        collection.AddHttpClient()
            .AddTransient<WebFingerResolver>()
            .AddTransient<WebFingerRenderer>();
        
        return collection;
    }
}