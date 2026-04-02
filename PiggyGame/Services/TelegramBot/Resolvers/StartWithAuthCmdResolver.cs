using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.Constants.TelegramMessages;
using PiggyGame.Data;
using PiggyGame.Services.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = PiggyGame.Data.Entities.User;
using AuthCode = PiggyGame.Data.Entities.AuthCode;

namespace PiggyGame.Services.TelegramBot.Resolvers;

public class StartWithAuthCmdResolver : IRequestHandler<StartWithAuthCmd>
{
    private readonly ApplicationConfiguration _configuration;

    private readonly IDbContext _dbContext;
    private readonly ITelegramBot _bot;
    private readonly IWebHostEnvironment _hostEnvironment;

    public StartWithAuthCmdResolver(
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

    public async Task Handle(StartWithAuthCmd request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == request.TelegramId);
        if (user == null)
        {
            user = new User
            {
                TelegramId = request.TelegramId,
                ChatId = request.ChatId,
                Username = request.Username,
                TreasuryUpdatedPopupShown = true
            };

            _dbContext.Users.Add(user);
        }
        else
        {
            user.ChatId = request.ChatId;
            user.Username = request.Username;

        }

        var authCode = await _dbContext.AuthCodes.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (authCode == null)
        {
            authCode = new AuthCode
            {
                UserId = user.Id,
                Code = Guid.NewGuid().ToString("N")
            };

            _dbContext.AuthCodes.Add(authCode);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var photoPath = Path.Join(_hostEnvironment.WebRootPath, "Images", "Welcome.png");
        await using var photoStream = new FileStream(photoPath, FileMode.Open);

        await _bot.Client.SendPhotoAsync(
            chatId: request.ChatId,
            photo: new InputFileStream(photoStream),
            caption: TelegramCommonMessages.AuthCode(authCode.Code),
            cancellationToken: cancellationToken
        );
    }
}
