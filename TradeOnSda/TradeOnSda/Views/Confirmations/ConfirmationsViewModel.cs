using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using SteamAuthentication.Exceptions;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using TradeOnSda.ViewModels;
using TradeOnSda.Views.ConfirmationItem;
using TradeOnSda.Windows.NotificationMessage;

namespace TradeOnSda.Views.Confirmations;

public class ConfirmationsViewModel : ViewModelBase
{
    public SdaConfirmation[] SdaConfirmations { get; private set; }

    public ObservableCollection<ConfirmationItemViewModel> ConfirmationsViewModels { get; }

    public bool VisibilityNoConfirmationsText { get; }

    public ICommand RefreshConfirmationsCommand { get; }

    public ConfirmationsViewModel(SdaConfirmation[] sdaConfirmations, SteamGuardAccount steamGuardAccount,
        Window ownerWindow)
    {
        SdaConfirmations = sdaConfirmations;

        VisibilityNoConfirmationsText = sdaConfirmations.Length == 0;

        ConfirmationsViewModels = new ObservableCollection<ConfirmationItemViewModel>(sdaConfirmations
            .Select(t => new ConfirmationItemViewModel(steamGuardAccount, t, ownerWindow, this)));

        RefreshConfirmationsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var newConfirmations = (await steamGuardAccount.FetchConfirmationAsync())
                    .Where(t => t.ConfirmationType is ConfirmationType.Trade or ConfirmationType.MarketSellTransaction)
                    .ToArray();

                SdaConfirmations = newConfirmations;
                
                ConfirmationsViewModels.Clear();

                foreach (var confirmation in newConfirmations)
                    ConfirmationsViewModels.Add(new ConfirmationItemViewModel(steamGuardAccount, confirmation,
                        ownerWindow, this));
            }
            catch (RequestException e)
            {
                await NotificationsMessageWindow.ShowWindow(
                    $"Error load confirmations, message: {e.Message}, httpStatusCode: {e.HttpStatusCode.ToString()}",
                    ownerWindow);
            }
            catch (Exception e)
            {
                await NotificationsMessageWindow.ShowWindow($"Error load confirmations, message: {e.Message}",
                    ownerWindow);
            }
        });
    }

    public void RemoveViewModel(ConfirmationItemViewModel viewModel)
    {
        ConfirmationsViewModels.Remove(viewModel);
    }

    public ConfirmationsViewModel()
    {
        SdaConfirmations = null!;
        ConfirmationsViewModels = null!;
        RefreshConfirmationsCommand = null!;
    }
}