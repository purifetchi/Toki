using Hangfire;
using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Formatters;
using Toki.ActivityPub.Jobs.Fetching;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Models.Users;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Users;

/// <summary>
/// A service that helps with managing users.
/// </summary>
public class UserManagementService(
    UserRepository repo,
    UserRenderer renderer,
    ActivityPubResolver resolver,
    MessageFederationService federationService,
    ContentFormatter contentFormatter,
    ILogger<UserManagementService> logger)
{
    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="updateRequest">The update request.</param>
    public async Task Update(
        User user,
        UserUpdateRequest updateRequest)
    {
        // TODO: Care about the limits.
        // TODO: Store the emojis from the bio and fields somewhere.
        
        if (updateRequest.Bio is not null)
        {
            var formattingResult = await contentFormatter.Format(
                updateRequest.Bio);

            if (formattingResult is not null)
                user.Bio = formattingResult.Formatted;
        }

        if (updateRequest.Fields is not null)
        {
            var fields = new List<UserProfileField>();
            foreach (var field in updateRequest.Fields)
            {
                if (field.Name is null || field.Value is null)
                    continue;
                
                var nameFormattingResult = await contentFormatter.Format(
                    field.Name);

                var valueFormattingResult = await contentFormatter.Format(
                    field.Value);

                if (nameFormattingResult is null || valueFormattingResult is null)
                    continue;
                
                fields.Add(new UserProfileField
                {
                    Name = nameFormattingResult.Formatted,
                    Value = valueFormattingResult.Formatted
                });
            }

            user.Fields = fields;
        }

        user.DisplayName = updateRequest.DisplayName ?? user.DisplayName;
        user.RequiresFollowApproval = updateRequest.RequiresFollowApproval ?? user.RequiresFollowApproval;

        if (updateRequest.RemoveAvatar)
            user.AvatarUrl = null;
        else
            user.AvatarUrl = updateRequest.AvatarUrl ?? user.AvatarUrl;
        
        if (updateRequest.RemoveHeader)
            user.BannerUrl = null;
        else
            user.BannerUrl = updateRequest.HeaderUrl ?? user.BannerUrl;
        
        await repo.Update(user);
        
        // Federate the message.
        var msg = await renderer.RenderUpdateFor(
            user);

        await federationService.SendToFollowers(user, msg);
    }
    
    /// <summary>
    /// Schedules an actor update if one is required.
    /// </summary>
    /// <param name="user">The actor to update.</param>
    private void ScheduleActorUpdateIfNecessary(User user)
    {
        const int maxRetentionInDays = 5;

        if (!user.IsRemote || user.RemoteId is null)
            return;
        
        var timeDiff = DateTimeOffset.UtcNow - user.LastUpdateTime;
        if (timeDiff.Days < maxRetentionInDays)
            return;

        logger.LogInformation($"Scheduling an actor update for {user.RemoteId}");
        BackgroundJob.Enqueue<UpdateActorDataJob>(
            job => job.UpdateActor(user.RemoteId!));
    }

    /// <summary>
    /// Fetches a user given their remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the user.</param>
    /// <returns>The user.</returns>
    public async Task<User?> FetchFromRemoteId(
        string remoteId)
    {
        var maybeUser = await repo.FindByRemoteId(remoteId);
        if (maybeUser is not null)
        {
            ScheduleActorUpdateIfNecessary(maybeUser);
            return maybeUser;
        }

        var actor = await resolver.Fetch<ASActor>(
            ASObject.Link(remoteId));

        if (actor is null)
            return null;

        var user = await repo.ImportFromActivityStreams(
            actor);
        
        if (user is null)
            return null;
        
        BackgroundJob.Enqueue<FillRemoteUserProfileJob>(job =>
            job.FillProfile(user.Id));
        
        return user;
    }
}