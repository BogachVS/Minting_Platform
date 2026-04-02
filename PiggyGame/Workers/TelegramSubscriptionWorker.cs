using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PiggyGame.Common.Configurations;
using PiggyGame.Data;
using PiggyGame.Data.Entities;
using PiggyGame.Services.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PiggyGame.Workers;

public class TelegramSubscriptionWorker : BackgroundService
{
    private readonly ITelegramBot _telegramBot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<TelegramNewsChannelConfiguration> _configuration;
    private readonly ILogger<TelegramSubscriptionWorker> _logger;

    public TelegramSubscriptionWorker(
        ITelegramBot telegramBot,
        IServiceScopeFactory scopeFactory,
        IOptions<TelegramNewsChannelConfiguration> configuration,
        ILogger<TelegramSubscriptionWorker> logger)
    {
        _telegramBot = telegramBot;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await CheckSubscriptions();
            await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
        }
    }

    private async Task CheckSubscriptions()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var usersList = await dbContext.Users.ToListAsync();
        
        foreach (var user in usersList)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await CheckUserSubscription(user);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while checking user with ID {user.Id} subscriptions");
            }
        }
        
        await dbContext.SaveChangesAsync();
    }

    private async Task CheckUserSubscription(User user)
    {
        var chatMember = await _telegramBot.Client.GetChatMemberAsync(_configuration.Value.ChannelId, user.TelegramId);
        var isSubscribed = chatMember.Status 
            is ChatMemberStatus.Member 
            or ChatMemberStatus.Administrator 
            or ChatMemberStatus.Creator;

        user.HasTelegramSubscription = isSubscribed;

        if (isSubscribed && !user.HasTelegramSubscriptionReward)
        {
            user.PigsAmount += 1000;
            user.TicketsAmount = (ulong)Math.Floor(user.PigsAmount / 1000m);
            user.HasTelegramSubscriptionReward = true;
        }
    }
}