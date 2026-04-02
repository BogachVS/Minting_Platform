namespace PiggyGame.Common.DTOs.Users;

public class ScoreboardUser
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    
    public required ulong PigsAmount { get; set; }
    public required ulong TicketsAmount { get; set; }
    
    public required int Referrals { get; set; }
}