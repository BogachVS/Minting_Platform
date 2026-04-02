using Newtonsoft.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PiggyGame.Common.Constants.TelegramBot;
using PiggyGame.Data;
using PiggyGame.Data.ValueObjects;
using PiggyGame.Data.ValueObjects.CallbackData;
using PiggyGame.Services.TelegramBot.Commands;
using Telegram.Bot.Types;

namespace PiggyGame.Services.TelegramBot;

public class TelegramBotCommandsResolver : ITelegramBotCommandsResolver
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramBotCommandsResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Resolve(Update update)
    {
        if (update.Message is { Text: not null })
        {
            var isResolved = await ResolveTextCommand(update);
            if (isResolved)
            {
                return;
            }
        }

        if (update.Message?.From?.Id != null)
        {
            await ResolveCommandsSequence(update);
        }

        if (update.CallbackQuery != null)
        {
            await ResolveCallbackQuery(update);
        }
    }

    private async Task<bool> ResolveTextCommand(Update update)
    {
        if (update.Message?.From?.Id == null || update.Message.From.Username == null)
        {
            return false;
        }

        IRequest? cmd = update.Message.Text switch
        {
            BotTextCommands.StartWithAuth => new StartWithAuthCmd
            {
                TelegramId = update.Message.From.Id,
                ChatId = update.Message.Chat.Id,
                Username = update.Message.From.Username
            },
            BotTextCommands.Start => new StartCmd
            {
                TelegramId = update.Message.From.Id,
                ChatId = update.Message.Chat.Id,
                Username = update.Message.From.Username
            },
            _ => null
        };

        if (cmd == null)
        {
            return false;
        }

        using var scope = _serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == update.Message.From.Id);
        if (user == null)
        {
            if (cmd is not StartCmd && cmd is not StartWithAuthCmd)
            {
                return false;
            }

            await mediator.Send(cmd);
            return true;
        }

        if (user.LastInteractionSequence.Name != BotSequenceCommands.Empty)
        {
            user.LastInteractionSequence = BotInteractionSequence.Empty();
            await dbContext.SaveChangesAsync();
        }

        await mediator.Send(cmd);

        return true;
    }

    private async Task ResolveCommandsSequence(Update update)
    {
        if (update.Message?.From == null || update.Message.Text == null)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == update.Message.From.Id);
        if (user == null)
        {
            return;
        }

        IRequest? cmd = user.LastInteractionSequence.Name switch
        {
            _ => null
        };

        if (cmd == null)
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(cmd);
    }

    private async Task ResolveCallbackQuery(Update update)
    {
        if (update.CallbackQuery?.Data == null || update.CallbackQuery?.Message?.Chat == null)
        {
            return;
        }

        var callbackData = JsonConvert.DeserializeObject<CallbackData>(update.CallbackQuery.Data);
        if (callbackData == null)
        {
            return;
        }

        IRequest? cmd = callbackData.Command switch
        {
            BotCallbackCommands.PrintRaffleRulesMessage => new PrintRaffleRulesMessageCmd { ChatId = update.CallbackQuery.Message.Chat.Id },
            _ => null,
        };

        if (cmd == null)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(cmd);
    }
}
