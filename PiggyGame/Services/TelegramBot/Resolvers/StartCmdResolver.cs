using MediatR;
using Microsoft.Extensions.Options;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.Constants.TelegramBot;
using PiggyGame.Common.Constants.TelegramMessages;
using PiggyGame.Data;
using PiggyGame.Data.ValueObjects.CallbackData;
using PiggyGame.Services.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = PiggyGame.Data.Entities.User;

namespace PiggyGame.Services.TelegramBot.Resolvers;

public class StartCmdResolver : IRequestHandler<StartCmd>
{
    private readonly ApplicationConfiguration _configuration;
    
    private readonly IDbContext _dbContext;
    private readonly ITelegramBot _bot;
    private readonly IWebHostEnvironment _hostEnvironment;

    public StartCmdResolver(
        IOptions<ApplicationConfiguration> configuration,
        ITelegramBot bot,
        IDbContext dbContext,
        IWebHostEnvironment hostEnvironment
    )
    {
        _configuration = configuration.Value;
        _bot = bot;
        _dbContext = dbContext;
        _hostEnvironment = hostEnvironment;
    }

    public async Task Handle(StartCmd request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(x => x.TelegramId == request.TelegramId);
        if (user == null)
        {
            user = new User
            {
                TelegramId = request.TelegramId,
                ChatId = request.ChatId,
                Username = request.Username,
                TreasuryUpdatedPopupShown = true
            };
            
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.ChatId = request.ChatId;
            user.Username = request.Username;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        var gameBtn = new InlineKeyboardButton("Start GAME") { WebApp = new WebAppInfo { Url = $"{_configuration.WebAppUrl}" } };
        var subscribeBtn = new InlineKeyboardButton("Подписаться на PiggyHODL News") { Url = _configuration.NewsUrl };
        var rulesBtn = new InlineKeyboardButton("Правила игры") { Url = _configuration.RulesUrl };
        var raffleRulesPostBtn = new InlineKeyboardButton("Как играть и выиграть копилку");
        
        var raffleRulesPostCallbackData = new CallbackData { Command = BotCallbackCommands.PrintRaffleRulesMessage };
        raffleRulesPostBtn.CallbackData = raffleRulesPostCallbackData.Serialize();
        
        var markup = new InlineKeyboardMarkup(new[] { new[] {gameBtn}, [subscribeBtn], [rulesBtn], [raffleRulesPostBtn] });

        var photoPath = Path.Join(_hostEnvironment.WebRootPath, "Images", "Welcome.png");
        await using var photoStream = new FileStream(photoPath, FileMode.Open);
        
        await _bot.Client.SendPhotoAsync(
            chatId: request.ChatId,
            photo: new InputFileStream(photoStream),
            caption: TelegramCommonMessages.Welcome(request.Username),
            replyMarkup: markup,
            cancellationToken: cancellationToken
        );
    }
}