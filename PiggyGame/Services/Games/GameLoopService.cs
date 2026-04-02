using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.DTOs.Games;
using PiggyGame.Common.Enums.Games;
using PiggyGame.Data;
using PiggyGame.Data.Entities;
using PiggyGame.Hubs;

namespace PiggyGame.Services.Games;

public class GameLoopService : IGameLoopService
{
    private readonly GameConfiguration _configuration;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ICollection<GameTask> _gameTasks;

    public GameLoopService(
        IOptions<GameConfiguration> configuration,
        IServiceProvider serviceProvider
    )
    {
        _configuration = configuration.Value;
        _serviceProvider = serviceProvider;
        _gameTasks = new List<GameTask>();
    }

    public void StartGame(string connectionId, Game game)
    {
        var existingGame = _gameTasks.FirstOrDefault(x => x.ConnectionId == connectionId);
        if (existingGame != null)
        {
            var gameValidTill = existingGame.Game.GameCreatedAt + TimeSpan.FromMilliseconds(_configuration.DurationInMs);
            if (gameValidTill < DateTime.UtcNow)
            {
                _gameTasks.Remove(existingGame);
            }
            else
            {
                throw new InvalidOperationException("Game's loop has already started.");
            }
        }

        var activeGame = CreateActivatedGame(connectionId, game);

        var gameCancellationToken = new GameCancellationToken();
        var gameLoopTask = RunGameLoop(activeGame, gameCancellationToken);
        var gameTask = new GameTask
        {
            ConnectionId = connectionId,
            Game = activeGame,
            Task = gameLoopTask,
            CancellationToken = gameCancellationToken
        };
        
        _gameTasks.Add(gameTask);
    }
    
    public void ResumeGame(string connectionId, Game game)
    {
        var existingGame = _gameTasks.FirstOrDefault(x => x.ConnectionId == connectionId);
        if (existingGame != null)
        {
            throw new InvalidOperationException("Game's loop already exists, cannot resume.");
        }
        
        var gameValidTill = game.CreatedAt + TimeSpan.FromMilliseconds(_configuration.DurationInMs);
        if (gameValidTill < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Game has already finished.");
        }
        
        var activeGame = CreateActivatedGame(connectionId, game);

        var gameCancellationToken = new GameCancellationToken();
        var gameLoopTask = RunGameLoop(activeGame, gameCancellationToken);
        var gameTask = new GameTask
        {
            ConnectionId = connectionId,
            Game = activeGame,
            Task = gameLoopTask,
            CancellationToken = gameCancellationToken
        };
        
        _gameTasks.Add(gameTask);
    }

    public void StopGame(string connectionId)
    {
        var existingGame = _gameTasks.FirstOrDefault(x => x.ConnectionId == connectionId);
        if (existingGame == null)
        {
            return;
        }

        existingGame.CancellationToken.IsCancellationRequested = true;
        existingGame.Task.Wait();
        
        _gameTasks.Remove(existingGame);
    }

    public void CatchPig(string connectionId, Guid pigId, int hole)
    {
        var existingGame = _gameTasks.FirstOrDefault(x => x.ConnectionId == connectionId);
        if (existingGame == null)
        {
            throw new InvalidOperationException("The game is not running.");
        }

        var pig = existingGame.Game.TotalPigs.FirstOrDefault(x => x.Id == pigId);
        if (pig == null || pig.Hole != hole)
        {
            throw new InvalidOperationException("Wrong hole was called.");
        }

        existingGame.Game.TotalPigs.Remove(pig);
        existingGame.Game.PigsCollected += 1;
    }

    private async Task RunGameLoop(ActivatedGame activatedGame, GameCancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var isClientDisconnected = false;
        var gameEndTime = activatedGame.GameCreatedAt + TimeSpan.FromMilliseconds(_configuration.DurationInMs);

        const int preStartDelayMs = 700;
        await Task.Delay(preStartDelayMs);
        
        while (gameEndTime >= DateTime.UtcNow && !isClientDisconnected)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            var holesToExclude = activatedGame.ActivePigs.Select(x => x.Hole).ToList();
            var spawnedPigs = SpawnPigs(holesToExclude);

            activatedGame.ActivePigs = spawnedPigs;
            activatedGame.TotalPigs.AddRange(spawnedPigs);
            
            try
            {
                var gameHub = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                var client = gameHub.Clients.Client(activatedGame.ConnectionId);
                
                await client.SendAsync("Receive", activatedGame.ActivePigs);
                
                var spawnInterval = GetGameCurrentSpawnInterval(activatedGame);
                await Task.Delay(spawnInterval);
            }
            catch
            {
                isClientDisconnected = true;
            }
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
        
        var game = await dbContext.Games.FirstAsync(x => x.Id == activatedGame.GameId);
        var user = await dbContext.Users.FirstAsync(x => x.Id == game.UserId);
        var userWithMaxPigsRecord = await dbContext.Users.OrderByDescending(x => x.MaxPigsAmount).FirstOrDefaultAsync();

        game.PigsCollected += activatedGame.PigsCollected;
        user.PigsAmount += activatedGame.PigsCollected;
        user.TicketsAmount = (ulong)Math.Floor(user.PigsAmount / 1000m);

        if (game.PigsCollected > user.MaxPigsAmount)
        {
            user.MaxPigsAmount = game.PigsCollected;
            user.NewRecordPopupShown = false;
        }

        if (userWithMaxPigsRecord != null && game.PigsCollected > userWithMaxPigsRecord.MaxPigsAmount)
        {
            const string query = $@"UPDATE ""{nameof(User)}"" SET ""{nameof(user.NewGlobalRecordPopupShown)}"" = FALSE;";
            await dbContext.DatabaseFacade.ExecuteSqlRawAsync(query);
        }

        await dbContext.SaveChangesAsync();

        if (!isClientDisconnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var gameHub = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                var client = gameHub.Clients.Client(activatedGame.ConnectionId);
                var newGameAvailableAt = game.CreatedAt + TimeSpan.FromMilliseconds(_configuration.DurationInMs) + TimeSpan.FromMilliseconds(_configuration.TimeoutIntervalInMs);
                
                await client.SendAsync(
                    "Receive", 
                    $"The game was gracefully finished! Pigs collected: {game.PigsCollected}. New game available at: {newGameAvailableAt:hh:mm:ss}",
                    game.PigsCollected,
                    ((DateTimeOffset)newGameAvailableAt).ToUnixTimeSeconds()
                    );
            }
            catch
            {
                // ignored
            }
        }
    }

    private ActivatedGame CreateActivatedGame(string connectionId, Game game)
    {
        var startInterval = GetValueFromIntervalWithStep(
            _configuration.SpawnStartIntervalMs.Min,
            _configuration.SpawnStartIntervalMs.Max,
            _configuration.SpawnStartIntervalMs.Step
        );
        
        var endInterval = GetValueFromIntervalWithStep(
            _configuration.SpawnEndIntervalMs.Min,
            _configuration.SpawnEndIntervalMs.Max,
            _configuration.SpawnEndIntervalMs.Step
        );
        
        return new ActivatedGame
        {
            ConnectionId = connectionId,
            GameId = game.Id,
            PigsCollected = 0,
            ActivePigs = [],
            TotalPigs = [],
            GameCreatedAt = game.CreatedAt,
            StartSpawnIntervalMs = startInterval,
            EndSpawnIntervalMs = endInterval
        };
    }
    
    private int GetGameCurrentSpawnInterval(ActivatedGame game)
    {
        var initialInterval = game.StartSpawnIntervalMs;
        var finalInterval = game.EndSpawnIntervalMs;
        
        var elapsedTime = (DateTime.UtcNow - game.GameCreatedAt).TotalMilliseconds;
        var elapsedRatio = elapsedTime / (_configuration.DurationInMs - 10_000);
        
        if (elapsedRatio > 1)
        {
            elapsedRatio = 1;
        }
        
        var easeInValue = Math.Pow(elapsedRatio, 3);
        var currentInterval = initialInterval - (initialInterval - finalInterval) * easeInValue;

        return (int)currentInterval;
    }

    private int GetHolePosition(ICollection<int> holesToExclude)
    {
        var range = Enumerable
            .Range(0, _configuration.HolesAmount)
            .Where(i => !holesToExclude.Contains(i));

        var random = new Random();
        var index = random.Next(0, _configuration.HolesAmount - holesToExclude.Count);
        
        return range.ElementAt(index);
    }
    
    private List<SpawnedPig> SpawnPigs(ICollection<int> holesToExclude)
    {
        var result = new List<SpawnedPig>();
        var random = new Random();
        var percents = random.Next(0, 100);

        if (percents < _configuration.DoublePigChanceInPercents)
        {
            var hole1 = GetHolePosition(holesToExclude);
            var hole2 = GetHolePosition([..holesToExclude, hole1]);

            var pig1 = new SpawnedPig
            {
                Id = Guid.NewGuid(),
                Hole = hole1,
                Type = PigType.Default
            };
            var pig2 = new SpawnedPig
            {
                Id = Guid.NewGuid(),
                Hole = hole2,
                Type = PigType.Default
            };
            
            result.AddRange([pig1, pig2]);
        }
        else
        {
            var hole = GetHolePosition(holesToExclude);
            var pig = new SpawnedPig
            {
                Id = Guid.NewGuid(),
                Hole = hole,
                Type = PigType.Default
            };
            
            result.Add(pig);
        }

        return result;
    }
    
    private static int GetValueFromIntervalWithStep(int min, int max, int step)
    {
        var random = new Random();

        min /= step;
        max /= step;

        return random.Next(min, max) * step;
    }
}