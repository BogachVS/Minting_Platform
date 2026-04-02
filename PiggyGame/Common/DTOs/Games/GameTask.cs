namespace PiggyGame.Common.DTOs.Games;

public class GameTask
{
    public required string ConnectionId { get; set; }
    public required ActivatedGame Game { get; set; }
    public required Task Task { get; set; }
    public required GameCancellationToken CancellationToken { get; set; }
}