using Hangfire;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using Toki.ActivityPub;
using Toki.ActivityPub.Configuration;
using Toki.HTTPSignatures;

var builder = WebApplication.CreateBuilder(args);

// Configure Toki
builder.Services.Configure<InstanceConfiguration>(
    builder.Configuration.GetSection("Instance"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddActivityPubServices();
builder.Services.AddHttpSignatures();
builder.Services.AddControllers();

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
app.UseHangfireDashboard();

app.MapControllers();

app.Run();