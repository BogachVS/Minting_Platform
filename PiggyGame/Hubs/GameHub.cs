using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PiggyGame.Common.Constants.Auth;
using PiggyGame.Services.Games;

namespace PiggyGame.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly IGameService _gameService;

    public GameHub(IGameService gameService)
    {
        _gameService = gameService;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var latestGame = await _gameService.GetLatestGameByUserId(userId);

        if (latestGame != null && latestGame.ValidTill > DateTime.UtcNow)
        {
            await Clients.Caller.SendAsync(
                "Receive", 
                $"You have incomplete game, which valid till: {latestGame.ValidTill:hh:mm:ss}", 
                ((DateTimeOffset)latestGame.ValidTill).ToUnixTimeSeconds()
            );      
        }
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        
        await _gameService.StopGame(Context.ConnectionId, userId);
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task StartGame()
    {
        var userId = GetUserId();
        var game = await _gameService.StartGame(Context.ConnectionId, userId);
        var gameValidTill = _gameService.GetGameValidTime(game);
        
        await Clients.Caller.SendAsync(
            "Receive", 
            $"Game was started. Valid till: {gameValidTill:hh:mm:ss}",
            ((DateTimeOffset)gameValidTill).ToUnixTimeSeconds()
        );
    }
    
    public async Task ResumeGame()
    {
        var userId = GetUserId();
        var game = await _gameService.ResumeGame(Context.ConnectionId, userId);
        var gameValidTill = _gameService.GetGameValidTime(game);
        
        await Clients.Caller.SendAsync(
            "Receive", 
            $"Game was resumed. Valid till: {gameValidTill:hh:mm:ss}",
            ((DateTimeOffset)gameValidTill).ToUnixTimeSeconds()
        );
    }

    public async Task CatchPig(string pigId, int hole)
    {
        var isParsed = Guid.TryParse(pigId, out var pigIdGuid);
        if (!isParsed)
        {
            throw new ArgumentException("Pig ID must be valid uuid");
        }
        
        _gameService.CatchPig(Context.ConnectionId, pigIdGuid, hole);
        
        await Clients.Caller.SendAsync("Receive", "Pig was catched!");
    }

    private int GetUserId()
    {
        var subClaim = Context.User?.Claims.FirstOrDefault(x => x.Type == JwtClaims.InternalId);
        if (subClaim == null)
        {
            throw new Exception("Not valid JWT: name identifier not provide");
        }
        
        var result = int.TryParse(subClaim.Value, out var id);
        if (!result)
        {
            throw new Exception("Not valid JWT: incorrect name identifier format");
        }
    
        return id;
    }
}
