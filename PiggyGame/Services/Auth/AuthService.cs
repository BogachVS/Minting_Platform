using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.DTOs.Auth;
using PiggyGame.Common.Exceptions;
using PiggyGame.Data;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Auth;

public class AuthService : IAuthService
{
    private readonly TelegramBotConfiguration _botConfiguration;
    private readonly IJwtService _jwtService;
    private readonly IDbContext _dbContext;

    public AuthService(
        IOptions<TelegramBotConfiguration> botConfiguration,
        IJwtService jwtService,
        IDbContext dbContext
    )
    {
        _botConfiguration = botConfiguration.Value;
        _jwtService = jwtService;
        _dbContext = dbContext;
    }

    public async Task<AccessToken> Authorize(string initData, string referralCode)
    {
        var initDataParsed = HttpUtility.ParseQueryString(initData);

        ValidateInitData(initDataParsed);

        var userData = GetUserAuthData(initDataParsed);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == userData.Id);
        if (user == null)
        {
            user = new User
            {
                TelegramId = userData.Id,
                Username = userData.Username,
                TreasuryUpdatedPopupShown = true
            };

            if (!string.IsNullOrEmpty(referralCode))
            {
                await ApplyReferralCode(referralCode, user);
            }

            await _dbContext.Users.AddAsync(user);
        }
        else
        {
            user.Username = userData.Username;
        }

        await _dbContext.SaveChangesAsync();

        return await _jwtService.GenerateAccessToken(user);
    }

    private void ValidateInitData(NameValueCollection initDataParsed)
    {
        var requestHash = initDataParsed["hash"];
        if (requestHash == null)
        {
            throw new UnauthorizedException();
        }

        var dataParams = initDataParsed.AllKeys
            .OrderBy(x => x)
            .Where(x => x != "hash")
            .Select(x => $"{x}={initDataParsed[x]}");
        var dataToCheck = string.Join("\n", dataParams);
        if (dataToCheck == null)
        {
            throw new UnauthorizedException();
        }

        var encoding = new UTF8Encoding();
        var key = HMACSHA256.HashData(encoding.GetBytes("WebAppData"), encoding.GetBytes(_botConfiguration.ClientToken));
        var hash = HMACSHA256.HashData(key, encoding.GetBytes(dataToCheck));

        var resultHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
        if (resultHash != requestHash)
        {
            throw new UnauthorizedException();
        }
    }

    private static AuthUserData GetUserAuthData(NameValueCollection initDataParsed)
    {
        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented
        };

        var userData = JsonConvert.DeserializeObject<AuthUserData>(initDataParsed["user"] ?? string.Empty, serializerSettings);
        if (userData == null)
        {
            throw new UnauthorizedException();
        }

        return userData;
    }

    private async Task ApplyReferralCode(string referralCode, User user)
    {
        var referer = await _dbContext.Users.FirstOrDefaultAsync(x => x.ReferralCode == referralCode);
        if (referer == null)
        {
            return;
        }

        user.Referrer = referer;
        referer.PigsAmount += 100;
        referer.TicketsAmount = (ulong)Math.Floor(referer.PigsAmount / 1000m);
        referer.Referrals.Add(user);
    }

    public async Task<AccessToken> AuthorizeWithCode(string code)
    {
        var authCode = await _dbContext.AuthCodes.FirstOrDefaultAsync(x => x.Code == code);
        if (authCode == null)
        {
            throw new BadHttpRequestException("Invalid auth code");
        }

        var accessToken = await _jwtService.GenerateAccessToken(authCode.User);

        _dbContext.AuthCodes.Remove(authCode);
        await _dbContext.SaveChangesAsync();

        return accessToken;
    }
}
