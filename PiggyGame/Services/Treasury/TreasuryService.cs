using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.Constants.Crypto;
using PiggyGame.Data;
using PiggyGame.Data.Entities;

namespace PiggyGame.Services.Treasury;

public class TreasuryService : ITreasuryService
{
    private readonly TonConfiguration _tonConfiguration;
    private readonly TreasuryConfiguration _treasuryConfiguration;
    private readonly IDbContext _dbContext;

    public TreasuryService(
        IOptions<TonConfiguration> tonConfiguration,
        IOptions<TreasuryConfiguration> treasuryConfiguration,
        IDbContext dbContext
    )
    {
        _tonConfiguration = tonConfiguration.Value;
        _treasuryConfiguration = treasuryConfiguration.Value;
        _dbContext = dbContext;
    }

    public async Task<double> GetTreasuryBalance()
    {
        var balance = await _dbContext.TreasuryBalances.FirstOrDefaultAsync();
        if (balance == null)
        {
            return 0;
        }
        
        return balance.Amount / Math.Pow(10, TokenDecimals.Ton);
    }

    public async Task UpdateTreasuryBalance()
    {
        using var httpClient = new HttpClient();
        
        var url = $"{_tonConfiguration.ApiUrl}/v2/getAddressBalance?address={_treasuryConfiguration.Address}";
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
            Headers = { { "X-API-Key", _tonConfiguration.ApiKey } }
        };
        
        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseContent);

        if (json["ok"]?.Value<bool>() != true)
        {
            return;
        }

        var rawBalance = json["result"]?.Value<string>();
        if (rawBalance == null)
        {
            return;
        }
        
        var balances = await _dbContext.TreasuryBalances.ToListAsync();
        _dbContext.TreasuryBalances.RemoveRange(balances);

        var amount = double.Parse(rawBalance);
        var balance = new TreasuryBalance { Amount = amount };
        
        await _dbContext.TreasuryBalances.AddAsync(balance);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ResetTreasuryUpdatedPopup()
    {
        var users = await _dbContext.Users.ToListAsync();
        
        foreach (var user in users)
        {
            user.TreasuryUpdatedPopupShown = false;
        }

        await _dbContext.SaveChangesAsync();
        await UpdateTreasuryBalance();
    }
}