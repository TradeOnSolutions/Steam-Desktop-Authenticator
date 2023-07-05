using System.Threading;
using System.Threading.Tasks;
using SteamAuthentication.GuardLinking;

namespace TradeOnSda.Views.AddGuardFirstStep;

// ReSharper disable once InconsistentNaming
public class UIPhoneProvider : IPhoneNumberProvider
{
    private readonly AddGuardViewModel _viewModel;

    public UIPhoneProvider(AddGuardViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    public async Task<string> GetPhoneNumberAsync(CancellationToken cancellationToken) => 
        await _viewModel.AskUserAsync("Enter a new phone number for steam account");
}