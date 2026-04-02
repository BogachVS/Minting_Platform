namespace PiggyGame.Common.DTOs.Auth;

public class AccessToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
