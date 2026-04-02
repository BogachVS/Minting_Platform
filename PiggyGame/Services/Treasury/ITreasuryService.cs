namespace PiggyGame.Services.Treasury;

public interface ITreasuryService
{
    public Task<double> GetTreasuryBalance();
    public Task UpdateTreasuryBalance();
    
    public Task ResetTreasuryUpdatedPopup();
}