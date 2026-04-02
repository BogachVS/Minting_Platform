using PiggyGame.Common.Enums.Games;

namespace PiggyGame.Common.DTOs.Games;

public class SpawnedPig
{
    public required Guid Id { get; set; }
    public required int Hole { get; set; }
    public required PigType Type { get; set; }
}