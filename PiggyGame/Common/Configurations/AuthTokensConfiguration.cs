namespace PiggyGame.Common.Configurations;

public class AuthTokensConfiguration
{
    public int AccessTokenLifeTimeInSeconds { get; set; }
    public string AccessTokenSecret { get; set; } = string.Empty;
    
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
