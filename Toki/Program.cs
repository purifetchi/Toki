using Toki.ActivityPub;
using Toki.ActivityPub.WebFinger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddActivityPubServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("account", async (WebFingerResolver resolver) =>
    {
        var result = await resolver.Finger("https://miku.place", "@pref_test@miku.place");
        return result;
    }).WithName("Account")
    .WithOpenApi();

app.Run();
