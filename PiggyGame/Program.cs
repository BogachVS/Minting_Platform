using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.WebExtensions;
using PiggyGame.Controllers.Middlewares;
using PiggyGame.Data;
using PiggyGame.Hubs;
using PiggyGame.Hubs.Filters;
using PiggyGame.Services;
using PiggyGame.Workers;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Inject project's dependencies

// --- Http ---
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddControllers()
    .SetCustomValidationErrorsFormat()
    .AddEnumSerialization();

// --- Docs ---
builder.Services.AddSwagger();

// --- Application ---
builder.Services.AddConfigurations(configuration);
builder.Services.AddDatabase(configuration);
builder.Services.AddApplicationServices(configuration);
builder.Services.AddWorkers(configuration);
builder.Services
    .AddSignalR(hubOptions =>
    {
        hubOptions.EnableDetailedErrors = true;
        hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
        hubOptions.AddFilter<HubExceptionsFilter>();
    })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Build the app
var app = builder.Build();

// --- Run scoped initial methods ---
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PostgresqlDbContext>();

await dbContext.Database.MigrateAsync();

// --- Configure the HTTP request pipeline ---
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/api/game-hub", options => {
    options.ApplicationMaxBufferSize = 128;
    options.TransportMaxBufferSize = 128;
    options.LongPolling.PollTimeout = TimeSpan.FromMinutes(1);
    options.Transports = HttpTransportType.LongPolling | HttpTransportType.WebSockets;
});

// --- Run the application ---
app.Run();