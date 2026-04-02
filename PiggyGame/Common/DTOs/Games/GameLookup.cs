namespace PiggyGame.Common.DTOs.Games;

public class GameLookup
{
    public int Id { get; set; }
    public uint PigsCollected { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime ValidTill { get; set; }
    public DateTime NextGameAvailableAt { get; set; }
}