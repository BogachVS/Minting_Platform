using PiggyGame.Common.DTOs.Games;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Games;

public interface IGameService
{
    public Task<GameLookup?> GetLatestGameByUserId(int userId);
    public DateTime GetGameValidTime(Game game);

    public Task<Game> StartGame(string connectionId, int userId);
    public Task<Game> ResumeGame(string connectionId, int userId);
    public Task StopGame(string connectionId, int userId);

    public void CatchPig(string connectionId, Guid pigId, int hole);
}