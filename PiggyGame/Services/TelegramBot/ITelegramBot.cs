using Telegram.Bot;

namespace PiggyGame.Services.TelegramBot;

public interface ITelegramBot
{
    public ITelegramBotClient Client { get; set; }
    public Task Listen(CancellationToken cancellationToken);
}