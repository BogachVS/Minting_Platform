using System.Reflection;
using PiggyGame.Services.Auth;
using PiggyGame.Services.Games;
using PiggyGame.Services.TelegramBot;
using PiggyGame.Services.Treasury;
using PiggyGame.Services.Users;

namespace PiggyGame.Services;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
        });
        
        serviceCollection.AddSingleton<ITelegramBot, TelegramBotWrapper>();
        serviceCollection.AddSingleton<ITelegramBotCommandsResolver, TelegramBotCommandsResolver>();
        serviceCollection.AddSingleton<IGameLoopService, GameLoopService>();

        serviceCollection.AddScoped<IAuthService, AuthService>();
        serviceCollection.AddScoped<IJwtService, JwtService>();
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IGameService, GameService>();
        serviceCollection.AddScoped<ITreasuryService, TreasuryService>();
    }
}