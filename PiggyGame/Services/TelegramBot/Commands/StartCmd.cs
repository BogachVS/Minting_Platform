using MediatR;

namespace PiggyGame.Services.TelegramBot.Commands;

public class StartCmd : IRequest
{
    public long TelegramId { get; set; }
    public long ChatId { get; set; }

    public string Username { get; set; } = string.Empty;
}
