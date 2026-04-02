using PiggyGame.Services.TelegramBot;

namespace PiggyGame.Workers;

public class TelegramBotWorker : BackgroundService
{
    private readonly ITelegramBot _bot;

    public TelegramBotWorker(ITelegramBot bot)
    {
        _bot = bot;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _bot.Listen(stoppingToken);
    }
}