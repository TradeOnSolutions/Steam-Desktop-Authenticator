using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using DynamicData.Binding;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;
using TradeOnSda.Windows.Confirmations;
using TradeOnSda.Windows.NotificationMessage;
using SteamTime = TradeOnSda.Data.SteamTime;

namespace TradeOnSda.Views.Account;

public class AccountViewModel : ViewModelBase
{
    private IAccountViewCommandStrategy _selectedAccountViewCommandStrategy = null!;
    private bool _isVisible;
    private bool _saveIconVisibility;
    private bool _rollbackIconVisibility;
    private bool _proxyIconVisibility;
    private bool _confirmationsIconVisibility;
    private bool _autoConfirm;
    private bool _isUnknownProxyState;
    private bool _isOkProxyState;
    private bool _isErrorProxyState;

    public SdaWithCredentials SdaWithCredentials { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set => RaiseAndSetIfPropertyChanged(ref _isVisible, value);
    }

    public bool SaveIconVisibility
    {
        get => _saveIconVisibility;
        set => RaiseAndSetIfPropertyChanged(ref _saveIconVisibility, value);
    }

    public bool RollbackIconVisibility
    {
        get => _rollbackIconVisibility;
        set => RaiseAndSetIfPropertyChanged(ref _rollbackIconVisibility, value);
    }

    public bool ProxyIconVisibility
    {
        get => _proxyIconVisibility;
        set => RaiseAndSetIfPropertyChanged(ref _proxyIconVisibility, value);
    }

    public bool ConfirmationsIconVisibility
    {
        get => _confirmationsIconVisibility;
        set => RaiseAndSetIfPropertyChanged(ref _confirmationsIconVisibility, value);
    }

    public bool AutoConfirm
    {
        get => _autoConfirm;
        set => RaiseAndSetIfPropertyChanged(ref _autoConfirm, value);
    }

    public bool IsUnknownProxyState
    {
        get => _isUnknownProxyState;
        set => RaiseAndSetIfPropertyChanged(ref _isUnknownProxyState, value);
    }

    public bool IsOkProxyState
    {
        get => _isOkProxyState;
        set => RaiseAndSetIfPropertyChanged(ref _isOkProxyState, value);
    }

    public bool IsErrorProxyState
    {
        get => _isErrorProxyState;
        set => RaiseAndSetIfPropertyChanged(ref _isErrorProxyState, value);
    }

    public ICommand ToggleAutoConfirmCommand { get; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public ICommand DoubleClickCommand { get; }

    public ICommand RemoveCommand { get; }

    public SdaManager SdaManager { get; }

    public Window OwnerWindow { get; }

    public string AccountName => SdaWithCredentials.SteamGuardAccount.MaFile.AccountName;

    public AccountViewModel(SdaWithCredentials sdaWithCredentials, SdaManager sdaManager, Window ownerWindow)
    {
        SdaWithCredentials = sdaWithCredentials;
        SdaManager = sdaManager;
        OwnerWindow = ownerWindow;
        IsVisible = true;
        AutoConfirm = sdaWithCredentials.SdaSettings.IsEnabledAutoConfirm;

        DefaultAccountViewCommandStrategy = new DefaultAccountViewCommandStrategy(this);
        EditProxyAccountViewCommandStrategy = new EditProxyAccountViewCommandStrategy(this);

        SelectStrategyAsync(DefaultAccountViewCommandStrategy).GetAwaiter().GetResult();

        IsUnknownProxyState = true;
        
        SdaWithCredentials.SdaState.WhenPropertyChanged(t => t.ProxyState)
            .Subscribe(valueWrapper =>
            {
                var newProxyState = valueWrapper.Value;

                switch (newProxyState)
                {
                    case ProxyState.Unknown:
                        IsUnknownProxyState = true;
                        IsOkProxyState = false;
                        IsErrorProxyState = false;
                        break;
                    case ProxyState.Ok:
                        IsUnknownProxyState = false;
                        IsOkProxyState = true;
                        IsErrorProxyState = false;
                        break;
                    case ProxyState.Error:
                        IsUnknownProxyState = false;
                        IsOkProxyState = false;
                        IsErrorProxyState = true;
                        break;
                }
            });
        
        FirstCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await SelectedAccountViewCommandStrategy.InvokeFirstCommandAsync();
        });

        SecondCommand = ReactiveCommand.Create(async () =>
        {
            await SelectedAccountViewCommandStrategy.InvokeSecondCommandAsync();
        });

        RemoveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await SdaManager.RemoveAccountAsync(SdaWithCredentials);
        });

        DoubleClickCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var confirmations = (await SdaWithCredentials.SteamGuardAccount.FetchConfirmationAsync()).Where(t =>
                    t.ConfirmationType is ConfirmationType.Trade or ConfirmationType.MarketSellTransaction
                        or ConfirmationType.Recovery).ToArray();

                var window = new ConfirmationsWindow(confirmations, SdaWithCredentials.SteamGuardAccount);

                window.Show();
            }
            catch (Exception)
            {
                await NotificationsMessageWindow.ShowWindow("Cannot load confirmations", OwnerWindow);
            }
        });

        ToggleAutoConfirmCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            AutoConfirm = !AutoConfirm;

            sdaWithCredentials.SdaSettings.IsEnabledAutoConfirm = AutoConfirm;

            await SdaManager.SaveSettingsAsync();
        });
    }

    public ICommand FirstCommand { get; }

    public ICommand SecondCommand { get; }

    public AccountViewModel()
    {
        SdaWithCredentials = null!;
        SdaManager = null!;
        OwnerWindow = null!;
        IsVisible = true;

        FirstCommand = null!;
        SecondCommand = null!;
        DoubleClickCommand = null!;
        ToggleAutoConfirmCommand = null!;

        DefaultAccountViewCommandStrategy = null!;
        EditProxyAccountViewCommandStrategy = null!;
        SelectedAccountViewCommandStrategy = null!;

        RemoveCommand = null!;
    }

    public async Task SelectStrategyAsync(IAccountViewCommandStrategy strategy)
    {
        SelectedAccountViewCommandStrategy = strategy;
        await strategy.OnSelectedAsync();
    }

    public IAccountViewCommandStrategy SelectedAccountViewCommandStrategy
    {
        get => _selectedAccountViewCommandStrategy;
        private set => RaiseAndSetIfPropertyChanged(ref _selectedAccountViewCommandStrategy, value);
    }

    public DefaultAccountViewCommandStrategy DefaultAccountViewCommandStrategy { get; }

    public EditProxyAccountViewCommandStrategy EditProxyAccountViewCommandStrategy { get; }
}

public interface IAccountViewCommandStrategy
{
    public bool IsVisibleLogin { get; }

    public bool IsVisibleTextBox { get; }

    public string? TextBoxText { get; set; }

    public Task InvokeFirstCommandAsync();

    public Task InvokeSecondCommandAsync();

    public Task OnSelectedAsync();
}

public class DefaultAccountViewCommandStrategy : IAccountViewCommandStrategy
{
    private readonly AccountViewModel _accountViewModel;

    public DefaultAccountViewCommandStrategy(AccountViewModel accountViewModel)
    {
        _accountViewModel = accountViewModel;
    }

    public bool IsVisibleLogin => true;
    public bool IsVisibleTextBox => false;
    public string? TextBoxText { get; set; } = string.Empty;

    public async Task InvokeFirstCommandAsync()
    {
        await _accountViewModel.SelectStrategyAsync(_accountViewModel.EditProxyAccountViewCommandStrategy);
    }

    public async Task InvokeSecondCommandAsync()
    {
        try
        {
            var confirmations = await _accountViewModel.SdaWithCredentials.SteamGuardAccount.FetchConfirmationAsync();

            confirmations = confirmations.Where(t =>
                t.ConfirmationType is ConfirmationType.Trade or ConfirmationType.MarketSellTransaction
                    or ConfirmationType.Recovery).ToArray();

            var window = new ConfirmationsWindow(confirmations, _accountViewModel.SdaWithCredentials.SteamGuardAccount);

            window.Show();
        }
        catch (Exception)
        {
            await NotificationsMessageWindow.ShowWindow("Cannot load confirmations", _accountViewModel.OwnerWindow);
        }
    }

    public Task OnSelectedAsync()
    {
        _accountViewModel.ProxyIconVisibility = true;
        _accountViewModel.ConfirmationsIconVisibility = true;
        _accountViewModel.SaveIconVisibility = false;
        _accountViewModel.RollbackIconVisibility = false;

        return Task.CompletedTask;
    }
}

public class EditProxyAccountViewCommandStrategy : ViewModelBase, IAccountViewCommandStrategy
{
    private readonly AccountViewModel _accountViewModel;
    private string? _textBoxText;

    public EditProxyAccountViewCommandStrategy(AccountViewModel accountViewModel)
    {
        _accountViewModel = accountViewModel;
    }

    public bool IsVisibleLogin => false;
    public bool IsVisibleTextBox => true;

    public string? TextBoxText
    {
        get => _textBoxText;
        set => RaiseAndSetIfPropertyChanged(ref _textBoxText, value);
    }

    public async Task InvokeFirstCommandAsync()
    {
        IWebProxy? proxy;

        if (string.IsNullOrWhiteSpace(TextBoxText))
            proxy = null;
        else
            try
            {
                proxy = ProxyLogic.ParseWebProxy(TextBoxText);
            }
            catch (Exception)
            {
                await NotificationsMessageWindow.ShowWindow("Proxy string is invalid", _accountViewModel.OwnerWindow);
                return;
            }

        _accountViewModel.SdaWithCredentials.Credentials.ProxyString = TextBoxText;
        _accountViewModel.SdaWithCredentials.Credentials.Proxy = proxy;

        var oldSda = _accountViewModel.SdaWithCredentials.SteamGuardAccount;
        var newSteamTime = new SteamTime();

        _accountViewModel.SdaWithCredentials.SteamGuardAccount = new SteamGuardAccount(oldSda.MaFile,
            new SteamRestClient(proxy), newSteamTime, NullLogger<SteamGuardAccount>.Instance);

        await _accountViewModel.SdaManager.SaveSettingsAsync();

        await _accountViewModel.SelectStrategyAsync(_accountViewModel.DefaultAccountViewCommandStrategy);
    }

    public async Task InvokeSecondCommandAsync()
    {
        await _accountViewModel.SelectStrategyAsync(_accountViewModel.DefaultAccountViewCommandStrategy);
    }

    public Task OnSelectedAsync()
    {
        TextBoxText = _accountViewModel.SdaWithCredentials.Credentials.ProxyString;

        _accountViewModel.ProxyIconVisibility = false;
        _accountViewModel.ConfirmationsIconVisibility = false;
        _accountViewModel.SaveIconVisibility = true;
        _accountViewModel.RollbackIconVisibility = true;

        return Task.CompletedTask;
    }
}