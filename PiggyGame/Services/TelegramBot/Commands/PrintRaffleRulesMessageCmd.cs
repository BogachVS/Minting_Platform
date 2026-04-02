using MediatR;

namespace PiggyGame.Services.TelegramBot.Commands;

public class PrintRaffleRulesMessageCmd : IRequest
{
    public long ChatId { get; set; }
}