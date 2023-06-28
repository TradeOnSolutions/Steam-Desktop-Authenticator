using Avalonia.Controls;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using TradeOnSda.ViewModels;
using TradeOnSda.Views.Confirmations;

namespace TradeOnSda.Windows.Confirmations;

public class ConfirmationsWindowViewModel : ViewModelBase
{
    public ConfirmationsViewModel ConfirmationsViewModel { get; }

    public ConfirmationsWindowViewModel(SdaConfirmation[] sdaConfirmations, SteamGuardAccount sda, Window ownerWindow)
    {
        ConfirmationsViewModel = new ConfirmationsViewModel(sdaConfirmations, sda, ownerWindow);
    }

    public ConfirmationsWindowViewModel()
    {
        ConfirmationsViewModel = null!;
    }
}