using Hangfire;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using Toki.ActivityPub;
using Toki.ActivityPub.Configuration;
using Toki.Binding.Extensions;
using Toki.HTTPSignatures;
using Toki.MastodonApi;
using Toki.Middleware.OAuth2;
using Toki.Services.Timelines;

var builder = WebApplication.CreateBuilder(args);

// Configure Toki
builder.Services.Configure<InstanceConfiguration>(
    builder.Configuration.GetSection("Instance"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddActivityPubServices();
builder.Services.AddHttpSignatures();
builder.Services.AddControllers( o => o.ModelBinderProviders.AddHybridBindingProvider());
builder.Services.AddTransient<OAuthMiddleware>();
builder.Services.AddTransient<TimelineBuilder>();

builder.Services.AddMastodonApiHelpers();

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(
        name: "MastodonAPI",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddHangfire(
        opts => opts.UseRedisStorage(ConnectionMultiplexer.Connect("localhost")))
    .AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<OAuthMiddleware>();
app.UseHangfireDashboard();

app.MapControllers();
app.MapRazorPages();

app.Run();