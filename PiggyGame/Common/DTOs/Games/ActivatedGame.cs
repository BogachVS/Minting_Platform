namespace PiggyGame.Common.DTOs.Games;

public class ActivatedGame
{
    public required int GameId { get; set; }
    public required string ConnectionId { get; set; }
    
    public required uint PigsCollected { get; set; }
    public required List<SpawnedPig> ActivePigs { get; set; }
    public required List<SpawnedPig> TotalPigs { get; set; }
    
    public required int StartSpawnIntervalMs { get; set; }
    public required int EndSpawnIntervalMs { get; set; }
    
    public required DateTime GameCreatedAt { get; set; }
}
