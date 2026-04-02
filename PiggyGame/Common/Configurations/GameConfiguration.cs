namespace PiggyGame.Common.Configurations;

public class GameConfiguration
{
    public int HolesAmount { get; set; }
    
    public int DurationInMs { get; set; }
    
    public int DoublePigChanceInPercents { get; set; }
    
    public int TimeoutIntervalInMs { get; set; }

    public SpawnInterval SpawnStartIntervalMs { get; set; } = new();
    public SpawnInterval SpawnEndIntervalMs { get; set; } = new();
}

public class SpawnInterval
{
    public int Min { get; set; }
    public int Max { get; set; }
    
    public int Step { get; set; }
}