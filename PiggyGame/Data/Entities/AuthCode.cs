namespace PiggyGame.Data.Entities;

public class AuthCode : Entity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
}
