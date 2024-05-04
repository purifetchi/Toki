using Ganss.Xss;
using Microsoft.Extensions.DependencyInjection;
using Toki.ActivityPub.Bites;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Emojis;
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
using Toki.ActivityStreams.Context;

namespace Toki.ActivityPub;

/// <summary>
/// Extensions for the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Sets the default JSON-LD context for ActivityPub messages.
    /// </summary>
    private static void SetDefaultLdContext()
    {
        LdContext.Default
            .AddLink("https://w3id.org/security/v1")
            .AddKeyValue("Bite", "https://ns.mia.jetzt/as#Bite")
            .AddKeyValue("Emoji", "toot:Emoji")
            .AddKeyValue("EmojiReact", "litepub:EmojiReact")
            .AddKeyValue("Hashtag", "as:Hashtag")
            .AddKeyValue("PropertyValue", "schema:PropertyValue")
            .AddKeyValue("fedibird", "http://fedibird.com/ns#")
            .AddKeyValue("sensitive", "as:sensitive")
            .AddKeyValue("toot", "http://joinmastodon.org/ns#")
            .AddKeyValue("value", "schema:value")
            .AddKeyValue("isCat", "https://misskey-hub.net/ns#isCat")
            .AddKeyValue("litepub", "http://litepub.social/ns#")
            .AddKeyValue("manuallyApprovesFollowers", "as:manuallyApprovesFollowers")
            .AddKeyValue("quoteUri", "fedibird:quoteUri")
            .AddKeyValue("schema", "http://schema.org#");
    }
    
    /// <summary>
    /// Adds the ActivityPub services.
    /// </summary>
    /// <param name="collection">The collection to add them to.</param>
    public static IServiceCollection AddActivityPubServices(this IServiceCollection collection)
    {
        SetDefaultLdContext();
        
        collection.AddDbContext<TokiDatabaseContext>();
        collection.AddScoped<InstanceActorResolver>();
        collection.AddTransient<ContentFormatter>();

        collection.AddScoped<InstanceRepository>();
        collection.AddScoped<BiteService>();

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
            .AddTransient<MessageFederationService>()
            .AddTransient<MicroformatsRenderer>();

        collection.AddTransient<EmojiRepository>()
            .AddTransient<EmojiService>();
        
        collection.AddHttpClient()
            .AddTransient<WebFingerResolver>()
            .AddTransient<WebFingerRenderer>();

        collection.AddSingleton<IHtmlSanitizer>(_ =>
        {
            var sanitizer = new HtmlSanitizer(new HtmlSanitizerOptions()
            {
                AllowedTags = new HashSet<string>([
                    "span", "br", "a", "p", "del", "pre", "code", "em", "strong", "b", "i", "u", "ul", "ol", "li",
                    "blockquote"
                ]),
                
                AllowedAttributes = HtmlSanitizerDefaults.AllowedAttributes,
                AllowedAtRules = HtmlSanitizerDefaults.AllowedAtRules,
                AllowedSchemes = HtmlSanitizerDefaults.AllowedSchemes,
                AllowedCssClasses = HtmlSanitizerDefaults.AllowedClasses,
                AllowedCssProperties = HtmlSanitizerDefaults.AllowedCssProperties,
                UriAttributes = HtmlSanitizerDefaults.UriAttributes
            });

            // TODO: Reduce the set of allowed tags.
            sanitizer.AllowedAttributes.Add("class");
            
            return sanitizer;
        });
        
        return collection;
    }
}