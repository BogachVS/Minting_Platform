using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Games;

public interface IGameLoopService
{
    public void StartGame(string connectionId, Game game);
    public void ResumeGame(string connectionId, Game game);
    public void StopGame(string connectionId);

    public void CatchPig(string connectionId, Guid pigId, int hole);
}