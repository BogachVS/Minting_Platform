using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.DTOs.Games;
using PiggyGame.Common.Exceptions;
using PiggyGame.Data;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Games;

public class GameService : IGameService
{
    private readonly GameConfiguration _configuration;

    private readonly IGameLoopService _gameLoopService;
    private readonly IDbContext _dbContext;

    public GameService(
        IOptions<GameConfiguration> configuration,
        IGameLoopService gameLoopService,
        IDbContext dbContext
    )
    {
        _configuration = configuration.Value;
        _gameLoopService = gameLoopService;
        _dbContext = dbContext;
    }
    
    public async Task<GameLookup?> GetLatestGameByUserId(int userId)
    {
        var game = await _dbContext.Games
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (game == null)
        {
            return null;
        }

        var validTill = game.CreatedAt + TimeSpan.FromMilliseconds(_configuration.DurationInMs);
        var nextGameAvailableAt = validTill + TimeSpan.FromMilliseconds(_configuration.TimeoutIntervalInMs);

        return new GameLookup
        {
            Id = game.Id,
            PigsCollected = game.PigsCollected,
            CreatedAt = game.CreatedAt,
            ValidTill = validTill,
            NextGameAvailableAt = nextGameAvailableAt
        };
    }

    public DateTime GetGameValidTime(Game game)
    {
        return game.CreatedAt + TimeSpan.FromMilliseconds(_configuration.DurationInMs);
    }
    
    public async Task<Game> StartGame(string connectionId, int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new EntityWasNotFoundException(nameof(User), nameof(userId), userId);
        }
        
        await EnsureGameIsAvailable(userId);

        var game = new Game { User = user };
        
        await _dbContext.Games.AddAsync(game);
        await _dbContext.SaveChangesAsync();
        
        _gameLoopService.StartGame(connectionId, game);

        return game;
    }
    
    public async Task<Game> ResumeGame(string connectionId, int userId)
    {
        var existingGame = await _dbContext.Games.FirstOrDefaultAsync(x => x.UserId == userId);
        if (existingGame == null)
        {
            throw new EntityWasNotFoundException(nameof(Game), nameof(userId), userId);
        }
        
        var gameValidTill = GetGameValidTime(existingGame);
        if (gameValidTill < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Nothing to resume.");
        }
        
        _gameLoopService.ResumeGame(connectionId, existingGame);

        return existingGame;
    }

    public async Task StopGame(string connectionId, int userId)
    {
        var existingGame = await _dbContext.Games.FirstOrDefaultAsync(x => x.UserId == userId);
        if (existingGame == null)
        {
            return;
        }
        
        _gameLoopService.StopGame(connectionId);
    }

    public void CatchPig(string connectionId, Guid pigId, int hole)
    {
        _gameLoopService.CatchPig(connectionId, pigId, hole);
    }

    private async Task EnsureGameIsAvailable(int userId)
    {
        var latestGame = await _dbContext.Games
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.UserId == userId);
        
        if (latestGame == null)
        {
            return;
        }

        var activeGameValidTill = GetGameValidTime(latestGame);
        if (activeGameValidTill > DateTime.UtcNow)
        {
            throw new InvalidOperationException($"You have an incomplete game, which valid till: {activeGameValidTill:hh:mm:ss}");
        }

        var newGameAvailableAt = activeGameValidTill + TimeSpan.FromMilliseconds(_configuration.TimeoutIntervalInMs);
        if (newGameAvailableAt > DateTime.UtcNow)
        {
            throw new InvalidOperationException($"Timeout is not over yet, a new game will be available at: {newGameAvailableAt:hh:mm:ss}");
        }

        _dbContext.Games.Remove(latestGame);
        await _dbContext.SaveChangesAsync();
    }
}