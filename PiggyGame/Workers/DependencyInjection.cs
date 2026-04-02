namespace PiggyGame.Workers;

public static class DependencyInjection
{
    public static void AddWorkers(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddHostedService<TelegramBotWorker>();
        serviceCollection.AddHostedService<TelegramSubscriptionWorker>();
    }
}