using PiggyGame.Common.DTOs.Auth;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Auth;

public interface IJwtService
{
    public Task<AccessToken> GenerateAccessToken(User user);
}
