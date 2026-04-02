namespace PiggyGame.Common.DTOs.Users;

public class ScoreboardPagedList
{
    public required ICollection<ScoreboardUser> Items { get; set; }
    public required int TotalItems { get; set; }
    public required int TotalActivePlayers { get; set; }
}