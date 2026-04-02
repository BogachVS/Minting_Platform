using Microsoft.EntityFrameworkCore;
using PiggyGame.Common.DTOs.Users;
using PiggyGame.Common.Exceptions;
using PiggyGame.Data;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Users;

public class UserService : IUserService
{
    private readonly IDbContext _dbContext;

    public UserService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ScoreboardPagedList> GetScoreBoard(int limit, int offset)
    {
        var totalActivePlayersQuery = _dbContext.Users.Where(x => x.PigsAmount >= 1000);
        var scoreboardQuery = _dbContext.Users
            .OrderByDescending(x => x.PigsAmount)
            .ThenByDescending(x => x.Games
                .OrderBy(y => y.UpdatedAt)
                .FirstOrDefault() != null
            )
            .ThenByDescending(x => x.Games
                .OrderBy(y => y.UpdatedAt)
                .FirstOrDefault()!
                .UpdatedAt
            )
            .Select(x => new ScoreboardUser
            {
                Id = x.Id,
                Username = x.Username,
                PigsAmount = x.PigsAmount,
                TicketsAmount = x.TicketsAmount,
                Referrals = x.Referrals.Count
            });

        return new ScoreboardPagedList
        {
            Items = await scoreboardQuery.Skip(offset).Take(limit).ToListAsync(),
            TotalItems = await scoreboardQuery.CountAsync(),
            TotalActivePlayers = await totalActivePlayersQuery.CountAsync()
        };
    }

    public async Task<PigsRecordDto> GetGlobalPigsRecord()
    {
        var userWithMaxPigs = await _dbContext.Users.OrderByDescending(x => x.MaxPigsAmount).FirstOrDefaultAsync();

        return new PigsRecordDto
        {
            MaxPigsAmount = userWithMaxPigs?.MaxPigsAmount ?? 0
        };
    }

    public async Task<UserDescription> GetUserById(int userId)
    {
        var user = await _dbContext.Users
            .Include(x => x.Referrals)
            .FirstOrDefaultAsync(x => x.Id == userId);
        
        if (user == null)
        {
            throw new EntityWasNotFoundException(nameof(User), nameof(userId), userId);
        }

        var scoreboardUsers = await _dbContext.Users
            .OrderByDescending(x => x.PigsAmount)
            .ToListAsync();
        
        if (string.IsNullOrEmpty(user.ReferralCode))
        {
            user.ReferralCode = $"{user.Id}-{Guid.NewGuid():N}";
            await _dbContext.SaveChangesAsync();
        }

        return new UserDescription
        {
            Id = user.Id,
            TelegramId = user.TelegramId,
            Username = user.Username,
            ScoreboardPosition = scoreboardUsers.IndexOf(user),
            PigsAmount = user.PigsAmount,
            MaxPigsAmount = user.MaxPigsAmount,
            TicketsAmount = user.TicketsAmount,
            Referrals = user.Referrals.Count,
            ReferralCode = user.ReferralCode,
            TreasuryUpdatedPopupShown = user.TreasuryUpdatedPopupShown,
            NewRecordPopupShown = user.NewRecordPopupShown,
            NewGlobalRecordPopupShown = user.NewGlobalRecordPopupShown
        };
    }

    public async Task UpdateUser(int userId, UpdateUserDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new EntityWasNotFoundException(nameof(User), nameof(userId), userId);
        }

        if (dto.TreasuryUpdatedPopupShown is { } treasuryUpdatedPopupShown)
        {
            user.TreasuryUpdatedPopupShown = treasuryUpdatedPopupShown;
        }
        
        if (dto.NewRecordPopupShown is { } newRecordPopupShown)
        {
            user.NewRecordPopupShown = newRecordPopupShown;
        }
        
        if (dto.NewGlobalRecordPopupShown is { } newGlobalRecordPopupShown)
        {
            user.NewGlobalRecordPopupShown = newGlobalRecordPopupShown;
        }

        await _dbContext.SaveChangesAsync();
    }
}