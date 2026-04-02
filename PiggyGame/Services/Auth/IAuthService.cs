using PiggyGame.Common.DTOs.Auth;

namespace PiggyGame.Services.Auth;

public interface IAuthService
{
    public Task<AccessToken> Authorize(string initData, string referralCode);
    public Task<AccessToken> AuthorizeWithCode(string code);
}
