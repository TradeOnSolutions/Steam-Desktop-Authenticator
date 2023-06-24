using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using TradeOnSda.ViewModels;
using TradeOnSda.Views.ConfirmationItem;

namespace TradeOnSda.Views.Confirmations;

public class ConfirmationsViewModel : ViewModelBase
{
    public SdaConfirmation[] SdaConfirmations { get; }

    public ObservableCollection<ConfirmationItemViewModel> ConfirmationsViewModels { get; }

    public bool VisibilityNoConfirmationsText { get; }

    public ConfirmationsViewModel(SdaConfirmation[] sdaConfirmations, SteamGuardAccount steamGuardAccount,
        Window ownerWindow)
    {
        SdaConfirmations = sdaConfirmations;

        VisibilityNoConfirmationsText = sdaConfirmations.Length == 0;

        ConfirmationsViewModels = new ObservableCollection<ConfirmationItemViewModel>(sdaConfirmations
            .Select(t => new ConfirmationItemViewModel(steamGuardAccount, t, ownerWindow, this)));
    }

    public void RemoveViewModel(ConfirmationItemViewModel viewModel)
    {
        ConfirmationsViewModels.Remove(viewModel);
    }

    public ConfirmationsViewModel()
    {
        SdaConfirmations = null!;
        ConfirmationsViewModels = null!;
    }
}