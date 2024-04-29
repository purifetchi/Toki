using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.Admin.Configuration;
using Toki.Configuration;

namespace Toki.Admin.Commands.Setup;

public class SetupHandler(
    IOptions<AdminConfiguration> adminOpts)
{
    private string PromptUser(
        string prompt,
        string defaultValue = "")
    {
        Console.Write(prompt + $" [{defaultValue}]: ");
        var ret = Console.ReadLine();

        return !string.IsNullOrEmpty(ret) ? ret : defaultValue;
    }
    
    public Task Handle(
        SetupOptions opts)
    {
        const int MB = 1024 * 1024;
        
        Console.WriteLine("Welcome! Answer a couple of questions to set up your Toki instance.");
        var domain = PromptUser("What should be the domain?", "toki.dev.zone.lol");
        var name = PromptUser("What should be the name of your instance?", "Toki Server");
        var description = PromptUser("What should be the description of your instance?", "A Toki server.");
        var email = PromptUser("What should be the default contact e-mail?", "toki@mail.com");
        var uploadPath = PromptUser("What should the path to uploads be?", "");
        var uploadLimit = PromptUser("What should the upload limit be (in megabytes)?", "10");
        
        var file = File.ReadAllText(adminOpts.Value.AppSettingsPath);
        var json = JsonNode.Parse(file)!;

        var instanceConfig = new InstanceConfiguration()
        {
            Name = name,
            Domain = domain,
            Info = description,
            ContactEmail = email
        };

        var uploadConfig = new UploadConfiguration()
        {
            UploadFolderPath = uploadPath,
            MaxFileSize = int.Parse(uploadLimit) * MB
        };

        json["Instance"] = JsonSerializer.SerializeToNode(instanceConfig);
        json["Upload"] = JsonSerializer.SerializeToNode(uploadConfig);
        
        File.WriteAllText(adminOpts.Value.AppSettingsPath, JsonSerializer.Serialize(json));
        return Task.CompletedTask;
    }
}