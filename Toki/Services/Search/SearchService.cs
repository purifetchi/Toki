using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityPub.WebFinger;
using Toki.ActivityStreams.Objects;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Requests.Search;
using Toki.MastodonApi.Schemas.Responses.Search;

namespace Toki.Services.Search;

/// <summary>
/// Service for searching through the Toki database.
/// </summary>
public class SearchService(
    UserRepository userRepo,
    AccountRenderer accountRenderer,
    PostRepository postRepo,
    UserManagementService userManagementService,
    PostManagementService postManagementService,
    WebFingerResolver webFingerResolver,
    ActivityPubResolver activityPubResolver,
    StatusRenderer statusRenderer,
    ILogger<SearchService> logger)
{
    /// <summary>
    /// The result limit.
    /// </summary>
    private const int LIMIT = 20;

    /// <summary>
    /// Handles maybe importing a remote note while searching.
    /// </summary>
    /// <param name="note">The note</param>
    /// <returns>The resulting post.</returns>
    private async Task<Post?> HandleRemoteImportForNote(ASNote note)
    {
        var maybePost = await postRepo.FindByRemoteId(note.Id);
        if (maybePost is not null)
            return maybePost;

        var author = await userManagementService.FetchFromRemoteId(note.AttributedTo!.Id);
        if (author is null)
            return null;
                    
        return await postManagementService.ImportFromActivityStreams(
            note,
            author);
    }
    
    /// <summary>
    /// Searches through the accounts.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="resolve">Resolve webfinger links?</param>
    /// <returns>The list of matching accounts.</returns>
    private async Task<List<Account>> SearchAccounts(
        string query,
        bool resolve)
    {
        var users = new List<User>();
        
        // First, try to get all the accounts from their @-symbols.
        var handles = new List<string>();
        var split = query.Split(' ');
        
        foreach (var part in split)
        {
            if (part.Length < 5)
                continue;

            if (part[0] != '@')
                continue;

            var handle = part[1..];
            handles.Add(handle);
        }
        
        // Fetch all handles at once.
        users.AddRange(await userRepo.CreateCustomQuery()
            .Where(u => handles.Contains(u.Handle))
            .ToListAsync());
        
        // Check every handle we didn't fetch from the query above and fetch it through webfinger.
        if (resolve)
        {
            foreach (var handle in handles
                         .Where(h => users.All(u => u.Handle != h)))
            {
                logger.LogInformation($"Fetching remote user {handle} for search.");
                
                try
                {
                    var result = await webFingerResolver.FingerAtHandle(handle);
                    var link = result?.Links
                        .FirstOrDefault(link => link.Type is not null &&
                                                link.Type.Contains("json"));

                    if (link?.Hyperlink is null)
                        continue;

                    var actor = await activityPubResolver.Fetch<ASActor>(
                        ASObject.Link(link.Hyperlink));

                    if (actor is null)
                        continue;

                    var user = await userRepo.ImportFromActivityStreams(actor);
                    if (user is null)
                        continue;

                    users.Add(user);
                }
                catch
                {
                    logger.LogInformation($"Remote user fetch failed for {handle}, skipping.");
                }
            }
        }
        
        // Finally run the query.
        users.AddRange(await userRepo.CreateCustomQuery()
            .Where(u => EF.Functions.Like(u.Handle, $"%{query}%"))
            .Take(LIMIT)
            .ToListAsync());

        return users
            .DistinctBy(u => u.Handle)
            .Select(u => accountRenderer.RenderAccountFrom(u))
            .ToList();
    }
    
    /// <summary>
    /// Searches through the posts.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="user">The user for whom we should render the posts.</param>
    /// <returns>The list of matching accounts.</returns>
    private async Task<List<Status>> SearchPosts(
        string query,
        User? user)
    {
        var posts = await postRepo.CreateCustomQuery()
            .Where(p => p.Boosting == null)
            .Where(p => EF.Functions.Like(p.Content, $"%{query}%"))
            .AddMastodonRenderNecessities()
            .Take(LIMIT)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        var statuses = await statusRenderer.RenderManyStatusesForUser(user, posts);
        return (statuses as List<Status>)!; // Don't tell anyone :)
    }

    /// <summary>
    /// Tries to resolve possible activity pub links from the query.
    /// </summary>
    /// <param name="query">The query string.</param>
    /// <param name="user">The user for whom we should render the posts.</param>
    /// <returns>The list of account and statuses resolved.</returns>
    private async Task<(List<Account>, List<Status>)> ResolvePossibleActivityPubLinks(
        string query,
        User? user)
    {
        var users = new List<User>();
        var posts = new List<Post>();

        var split = query.Split(' ');

        foreach (var value in split)
        {
            // If we don't start with HTTP, continue.
            if (!value.StartsWith("http"))
                continue;
            
            ASObject? asObject;
            try
            {
                asObject = await activityPubResolver.Fetch<ASObject>(
                    ASObject.Link(value));
            }
            catch
            {
                continue;
            }

            if (asObject is null)
                continue;

            logger.LogInformation($"Resolving object {value} for search.");
            switch (asObject)
            {
                case ASActor actor:
                    var maybeUser = await userRepo.FindByRemoteId(actor.Id);
                    var resolvedUser = maybeUser ?? await userRepo.ImportFromActivityStreams(actor);
                    if (resolvedUser is null)
                        continue;
                    
                    users.Add(resolvedUser);
                    break;

                case ASNote note:
                {
                    var post = await HandleRemoteImportForNote(note);

                    if (post is null)
                        continue;
                    
                    posts.Add(post);
                    break;
                }

                case ASVideo video:
                {
                    var post = await HandleRemoteImportForNote(
                        video.MockASNote());

                    if (post is null)
                        continue;
                    
                    posts.Add(post);
                    break;
                }
                
                default:
                    continue;
            }
        }

        var accounts = users
            .Select(u => accountRenderer.RenderAccountFrom(u))
            .ToList();

        var statuses = await statusRenderer.RenderManyStatusesForUser(
            user,
            posts);
        
        return (accounts, (statuses as List<Status>)!);
    }
    
    /// <summary>
    /// Runs a search query.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="user">The user who initiated the search.</param>
    /// <returns>The search response.</returns>
    public async Task<SearchResponse> Search(
        SearchRequest request,
        User? user = null)
    {
        var resolve = request.Resolve && user is not null;
        
        var accounts = await SearchAccounts(
            request.Query,
            resolve);

        var statuses = await SearchPosts(
            request.Query,
            user);
        
        if (resolve)
        {
            var (resolvedAccounts, resolvedStatuses) = await ResolvePossibleActivityPubLinks(
                request.Query,
                user);
            
            accounts.AddRange(resolvedAccounts);
            statuses.AddRange(resolvedStatuses);
        }

        return new SearchResponse
        {
            Accounts = accounts,
            Statuses = statuses
        };
    }
}