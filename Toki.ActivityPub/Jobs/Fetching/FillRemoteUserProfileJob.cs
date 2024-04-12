using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Jobs.Fetching;

/// <summary>
/// Job that tries to backfill the remote user's profile.
/// </summary>
public class FillRemoteUserProfileJob(
    ActivityPubResolver resolver,
    UserRepository userRepository,
    PostManagementService postManagementService,
    ILogger<FillRemoteUserProfileJob> logger)
{
    /// <summary>
    /// Fills the profile of the remote user.
    /// </summary>
    /// <param name="userId">The user's id.</param>
    public async Task FillProfile(
        Ulid userId)
    {
        var user = await userRepository.FindById(
            userId);

        if (user is null)
            return;
        
        var actor = await resolver.Fetch<ASActor>(
            ASObject.Link(user.RemoteId!));

        if (actor is null)
            return;

        logger.LogInformation($"Synchronizing information about remote user {user.Handle} ({user.RemoteId})");
        
        if (actor.Following is not null)
        {
            var following = await resolver.Fetch<ASCollection<ASObject>>(
                actor.Following);
            
            if (following is not null)
                user.FollowingCount = following.TotalItems;
        }
        
        if (actor.Followers is not null)
        {
            var followers = await resolver.Fetch<ASCollection<ASObject>>(
                actor.Followers);
            
            if (followers is not null)
                user.FollowerCount = followers.TotalItems;
        }
        
        // TODO: Also already sync the relations between users we already have here.

        if (actor.Featured is not null)
        {
            var pinned = await resolver.FetchCollection(
                actor.Featured);

            if (pinned is not null)
            {
                foreach (var pinObject in pinned)
                {
                    var post = await postManagementService.FetchFromRemoteId(pinObject.Id);
                    if (post is null)
                        continue;

                    await postManagementService.Pin(post);
                }
            }
        }

        await userRepository.Update(
            user);
    }
}