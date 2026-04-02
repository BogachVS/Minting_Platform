using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PiggyGame.Common.Configurations;
using PiggyGame.Common.Exceptions;
using PiggyGame.Services.Treasury;

namespace PiggyGame.Controllers;

public class TreasuryController : ApiController
{
    private readonly string _adminApiKey;
    private readonly ITreasuryService _treasuryService;

    public TreasuryController(
        IOptions<AdminConfiguration> adminConfiguration,
        ITreasuryService treasuryService
    )
    {
        _adminApiKey = adminConfiguration.Value.ApiKey;
        _treasuryService = treasuryService;
    }

    [HttpGet("balance")]
    public async Task<double> GetTreasuryBalance()
    {
        return await _treasuryService.GetTreasuryBalance();
    }

    [HttpPost("reset-notifications")]
    public async Task ResetTreasuryPopup([FromHeader] string apiKey)
    {
        if (_adminApiKey != apiKey)
        {
            throw new UnauthorizedException();
        }
        
        await _treasuryService.ResetTreasuryUpdatedPopup();
    }
}