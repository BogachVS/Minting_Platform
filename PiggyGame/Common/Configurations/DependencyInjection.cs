namespace PiggyGame.Common.Configurations;

public static class DependencyInjection
{
    public static void AddConfigurations(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<ApplicationConfiguration>(configuration.GetSection("Application").Bind);
        serviceCollection.Configure<AdminConfiguration>(configuration.GetSection("Admin").Bind);
        serviceCollection.Configure<TelegramBotConfiguration>(configuration.GetSection("TelegramBot").Bind);
        serviceCollection.Configure<AuthTokensConfiguration>(configuration.GetSection("AuthTokens").Bind);
        serviceCollection.Configure<GameConfiguration>(configuration.GetSection("Game").Bind);
        serviceCollection.Configure<TonConfiguration>(configuration.GetSection("Ton").Bind);
        serviceCollection.Configure<TreasuryConfiguration>(configuration.GetSection("Treasury").Bind);
        serviceCollection.Configure<TelegramNewsChannelConfiguration>(configuration.GetSection("TelegramNewsChannel").Bind);
    }
}