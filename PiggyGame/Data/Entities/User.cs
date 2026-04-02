using PiggyGame.Data.ValueObjects;

namespace PiggyGame.Data.Entities;

public class User : Entity
{
    public long TelegramId { get; set; }
    public long? ChatId { get; set; }

    public string Username { get; set; } = string.Empty;

    public ulong PigsAmount { get; set; }
    public ulong MaxPigsAmount { get; set; }
    public ulong TicketsAmount { get; set; }

    public string ReferralCode { get; set; } = string.Empty;

    public int? ReferrerId { get; set; }
    public User? Referrer { get; set; }
    public ICollection<User> Referrals { get; set; } = new List<User>();

    public ICollection<Game> Games { get; set; } = new List<Game>();

    public AuthCode? AuthCode { get; set; }

    public bool TreasuryUpdatedPopupShown { get; set; } = true;
    public bool NewRecordPopupShown { get; set; } = true;
    public bool NewGlobalRecordPopupShown { get; set; } = true;

    public bool HasTelegramSubscription { get; set; } = false;
    public bool HasTelegramSubscriptionReward { get; set; } = false;

    public BotInteractionSequence LastInteractionSequence { get; set; } = BotInteractionSequence.Empty();
}
