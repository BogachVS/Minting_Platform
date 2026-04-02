namespace PiggyGame.Common.DTOs.Users;

public class UserDescription
{
    public required int Id { get; set; }
    public required long TelegramId { get; set; }

    public required string Username { get; set; }
    
    public required int ScoreboardPosition { get; set; }
    public required ulong PigsAmount { get; set; }
    public required ulong MaxPigsAmount { get; set; }
    public required ulong TicketsAmount { get; set; }
    
    public required int Referrals { get; set; }
    public required string ReferralCode { get; set; }
    
    public required bool TreasuryUpdatedPopupShown { get; set; }
    public required bool NewRecordPopupShown { get; set; } = true;
    public required bool NewGlobalRecordPopupShown { get; set; } = true;
}