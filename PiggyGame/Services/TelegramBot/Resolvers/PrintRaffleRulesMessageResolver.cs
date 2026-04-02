using MediatR;
using PiggyGame.Common.Constants.TelegramMessages;
using PiggyGame.Services.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PiggyGame.Services.TelegramBot.Resolvers;

public class PrintRaffleRulesMessageResolver : IRequestHandler<PrintRaffleRulesMessageCmd>
{
    private readonly ITelegramBot _bot;

    public PrintRaffleRulesMessageResolver(ITelegramBot bot)
    {
        _bot = bot;
    }

    public async Task Handle(PrintRaffleRulesMessageCmd request, CancellationToken cancellationToken)
    {
        await _bot.Client.SendTextMessageAsync(
            chatId: request.ChatId,
            text: TelegramCommonMessages.RaffleRules(),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken
        );
    }
}