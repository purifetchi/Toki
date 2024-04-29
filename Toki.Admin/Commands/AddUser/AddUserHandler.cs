using Toki.ActivityPub.Persistence.Repositories;

namespace Toki.Admin.Commands.AddUser;

/// <summary>
/// The handler for the adduser command.
/// </summary>
/// <param name="repo">The user repository.</param>
public class AddUserHandler(
    UserRepository repo)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="opts">The command options.</param>
    public async Task Handle(AddUserOptions opts)
    {
        var user = await repo.CreateNewUser(
            opts.Username, 
            opts.Password);

        if (user is not null)
        {
            Console.WriteLine($"Created user with id of {user.Id}");
        }
    }
}