using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiggyGame.Common.DTOs.Games;
using PiggyGame.Common.Exceptions;
using PiggyGame.Services.Games;

namespace PiggyGame.Controllers;

[Authorize]
public class GameController : ApiController
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("latest")]
    public async Task<GameLookup> GetUsersLatestGame()
    {
        var userId = GetUserId();
        var latestGame = await _gameService.GetLatestGameByUserId(userId);

        if (latestGame == null)
        {
            throw new EntityWasNotFoundException("No data about user's last game was recorded.");
        }

        return latestGame;
    }
}