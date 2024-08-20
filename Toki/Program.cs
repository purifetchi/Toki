using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Toki.ActivityPub;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.Binding.Extensions;
using Toki.Configuration;
using Toki.HTTPSignatures;
using Toki.MastodonApi;
using Toki.Middleware.OAuth2;
using Toki.Services.Drive;
using Toki.Services.Search;
using Toki.Services.Timelines;
using Toki.Services.Usage;

var builder = WebApplication.CreateBuilder(args);

// Configure Toki
builder.Services.Configure<InstanceConfiguration>(
    builder.Configuration.GetSection("Instance"));

builder.Services.Configure<UploadConfiguration>(
    builder.Configuration.GetSection("Upload"));

builder.Services.Configure<FrontendConfiguration>(
    builder.Configuration.GetSection("Frontend"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddActivityPubServices();
builder.Services.AddHttpSignatures();
builder.Services.AddControllers( o => o.ModelBinderProviders.AddHybridBindingProvider());
builder.Services.AddStackExchangeRedisCache(o => { o.Configuration = "localhost"; });
builder.Services.AddTransient<OAuthMiddleware>();
builder.Services.AddTransient<TimelineBuilder>();
builder.Services.AddTransient<DriveService>();
builder.Services.AddTransient<UsageService>();
builder.Services.AddTransient<SearchService>();

builder.Services.AddMastodonApiHelpers();

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(
        name: "MastodonAPI",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Link");
        });
});

builder.Services.AddHangfire(
        opts => opts.UseRedisStorage(ConnectionMultiplexer.Connect("localhost")))
    .AddHangfireServer();

var app = builder.Build();

// Set the version, if the config didn't override it.
app.Services.GetRequiredService<IOptions<InstanceConfiguration>>()
    .Value
    .Software
    .SoftwareVersion ??= $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var uploadConfig = app.Services
    .GetRequiredService<IOptions<UploadConfiguration>>()
    .Value;

if (uploadConfig.UploadFolderPath is null)
{
    Console.WriteLine($"[ERROR] Upload folder isn't configured! Please set it in appsettings.json.");
    Environment.Exit(1);    
}

var uploadPath = Path.GetFullPath(uploadConfig.UploadFolderPath);
if (!Directory.Exists(uploadPath))
{
    Console.WriteLine($"[ERROR] The upload path ({uploadPath}) does not exist.");
    Environment.Exit(1);
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/media"
});
app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<OAuthMiddleware>();
app.UseHangfireDashboard();

app.MapControllers();

var frontend = app.Services
    .GetRequiredService<IOptions<FrontendConfiguration>>()
    .Value;

app.MapRazorPages();

if (frontend.Enabled)
{
    app.MapFallbackToFile(frontend.SpaFilename!);
}
else
{
    app.MapGet("/", async ctx =>
    {
        ctx.Response.Redirect("/Landing", true);
    });
}

// Perform migrations.
if (args.Any(a => a == "--auto-migrate"))
{
    var logger = app.Services
        .GetRequiredService<ILogger<TokiDatabaseContext>>();
    
    logger.LogInformation("Performing auto migrations...");

    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<TokiDatabaseContext>();

    db.Database.Migrate();
}

app.Run();
