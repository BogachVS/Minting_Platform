namespace PiggyGame.Data.Entities;

public class Game : Entity
{
    public int UserId { get; set; }
    public User User { get; set; } = new();
    
    public uint PigsCollected { get; set; }
}