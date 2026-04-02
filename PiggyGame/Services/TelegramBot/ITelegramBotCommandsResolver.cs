using Telegram.Bot.Types;

namespace PiggyGame.Services.TelegramBot;

public interface ITelegramBotCommandsResolver
{
    public Task Resolve(Update update);
}