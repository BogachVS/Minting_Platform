namespace PiggyGame.Common.DTOs.Auth;

public class AuthUserData
{
    public long Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    public string LanguageCode { get; set; } = string.Empty;
    
    public bool IsPremium { get; set; }
    
    public bool AllowsWriteToPm { get; set; }
}