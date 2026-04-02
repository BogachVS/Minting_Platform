using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PiggyGame.Common.Configurations;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PiggyGame.Services.TelegramBot;

public class TelegramBotWrapper : ITelegramBot
{
    public ITelegramBotClient Client { get; set; }
    private readonly ITelegramBotCommandsResolver _commandsResolver;
    private readonly ILogger<TelegramBotWrapper> _logger;
    
    public TelegramBotWrapper(
        ITelegramBotCommandsResolver commandsResolver,
        IOptions<TelegramBotConfiguration> configuration,
        ILogger<TelegramBotWrapper> logger
    )
    {
        _logger = logger;
        _commandsResolver = commandsResolver;
        
        Client = new TelegramBotClient(configuration.Value.ClientToken);
    }
    
    public async Task Listen(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        Client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );
        
        var botUser = await Client.GetMeAsync(cancellationToken);
        _logger.LogInformation($"Start listening for @{botUser.Username}");
    }
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Development")
        {
            _logger.LogInformation(JsonConvert.SerializeObject(update));   
        }

        try
        {
            await _commandsResolver.Resolve(update);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception was thrown from telegram bot commands resolver.");
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An exception was throw while polling telegram updates.");
        
        return Task.CompletedTask;
    }
}