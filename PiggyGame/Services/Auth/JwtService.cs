using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.Constants.Auth;
using PiggyGame.Common.DTOs.Auth;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Auth;

public class JwtService : IJwtService
{
    private readonly AuthTokensConfiguration _tokensConfiguration;

    public JwtService(IOptions<AuthTokensConfiguration> tokensConfiguration)
    {
        _tokensConfiguration = tokensConfiguration.Value;
    }

    public Task<AccessToken> GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtClaims.Sub, user.TelegramId.ToString()),
            new Claim(JwtClaims.InternalId, user.Id.ToString()),
            new Claim(JwtClaims.Username, user.Username)
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokensConfiguration.AccessTokenSecret));
        var expirationDate = DateTime.UtcNow.AddSeconds(_tokensConfiguration.AccessTokenLifeTimeInSeconds);
        
        var token = new JwtSecurityToken(
            issuer: _tokensConfiguration.Issuer,
            audience: _tokensConfiguration.Audience,
            expires: expirationDate,
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var result = new AccessToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expirationDate
        };

        return Task.FromResult(result);
    }
}
