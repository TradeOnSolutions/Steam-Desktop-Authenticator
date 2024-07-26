using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using DynamicData.Binding;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using SteamAuthentication.Exceptions;
using SteamAuthentication.Logic;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using SteamKit2.Internal;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;
using TradeOnSda.Windows.Confirmations;
using TradeOnSda.Windows.NotificationMessage;

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
    private bool _isContextMenuOpen;
    private string _autoConfirmDelayText = null!;
    private bool _isSuccessCommitAutoConfirmDelay;

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

    public bool IsContextMenuOpen
    {
        get => _isContextMenuOpen;
        set => RaiseAndSetIfPropertyChanged(ref _isContextMenuOpen, value);
    }

    public string AutoConfirmDelayText
    {
        get => _autoConfirmDelayText;
        set => RaiseAndSetIfPropertyChanged(ref _autoConfirmDelayText, value);
    }

    public bool IsSuccessCommitAutoConfirmDelay
    {
        get => _isSuccessCommitAutoConfirmDelay;
        set => RaiseAndSetIfPropertyChanged(ref _isSuccessCommitAutoConfirmDelay, value);
    }

    public ICommand CommitAutoConfirmDelayCommand { get; }

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
                var steamGuardAccount = SdaWithCredentials.SteamGuardAccount;

                if (steamGuardAccount.MaFile?.Session is SteamSessionData sessionData)
                {
                    var accessToken = sessionData.SteamLoginSecure?.Split("%7C%7C")[1];
                    var refreshToken = sessionData.RefreshToken;

                    if (refreshToken is not null &&
                        accessToken is not null &&
                        JwtTokenValidator.IsTokenExpired(accessToken))
                    {
                        var steamId = sessionData.SteamId;

                        var result =
                            await steamGuardAccount.GenerateAccessTokenAsync(steamId, refreshToken);

                        await SdaManager.SaveMaFile(steamGuardAccount);
                    }
                }

                var confirmations = (await steamGuardAccount.FetchConfirmationAsync()).Where(t =>
                    t.ConfirmationType is ConfirmationType.Trade or ConfirmationType.MarketSellTransaction
                        or ConfirmationType.Recovery).ToArray();

                var window = new ConfirmationsWindow(confirmations, steamGuardAccount);

                window.Show();
            }
            catch (RequestException e)
            {
                await NotificationsMessageWindow.ShowWindow($"Cannot load confirmations. {e}", OwnerWindow);
            }
            catch (Exception e)
            {
                await NotificationsMessageWindow.ShowWindow($"Cannot load confirmations, message: {e.Message}", OwnerWindow);
            }
        });

        ToggleAutoConfirmCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            AutoConfirm = !AutoConfirm;

            sdaWithCredentials.SdaSettings.IsEnabledAutoConfirm = AutoConfirm;

            await SdaManager.SaveSettingsAsync();
        });

        this.WhenPropertyChanged(t => t.IsContextMenuOpen)
            .Subscribe(valueWrapper =>
            {
                var newValue = valueWrapper.Value;

                if (newValue)
                    AutoConfirmDelayText =
                        ((int)sdaWithCredentials.SdaSettings.AutoConfirmDelay.TotalSeconds).ToString();
            });

        CommitAutoConfirmDelayCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (!int.TryParse(AutoConfirmDelayText, out var delay))
            {
                await NotificationsMessageWindow.ShowWindow("Error parse value", OwnerWindow);
                return;
            }

            if (delay < 1)
            {
                await NotificationsMessageWindow.ShowWindow("Delay must be positive", OwnerWindow);
                return;
            }

            sdaWithCredentials.SdaSettings.AutoConfirmDelay = TimeSpan.FromSeconds(delay);

            await SdaManager.SaveSettingsAsync();

            var _ = Task.Run(async () =>
            {
                IsSuccessCommitAutoConfirmDelay = true;
                await Task.Delay(TimeSpan.FromSeconds(1));
                IsSuccessCommitAutoConfirmDelay = false;
            });
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

        CommitAutoConfirmDelayCommand = null!;
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
            var steamGuardAccount = _accountViewModel.SdaWithCredentials.SteamGuardAccount;

            if (steamGuardAccount.MaFile?.Session is SteamSessionData sessionData)
            {
                var accessToken = sessionData.SteamLoginSecure?.Split("%7C%7C")[1];
                var refreshToken = sessionData.RefreshToken;

                if (refreshToken is not null &&
                    accessToken is not null &&
                    JwtTokenValidator.IsTokenExpired(accessToken))
                {
                    var steamId = sessionData.SteamId;

                    var result =
                        await steamGuardAccount.GenerateAccessTokenAsync(steamId, refreshToken);

                    await _accountViewModel.SdaManager.SaveMaFile(steamGuardAccount);
                }
            }

            var confirmations = await steamGuardAccount.FetchConfirmationAsync();

            confirmations = confirmations.Where(t =>
                t.ConfirmationType is ConfirmationType.Trade or ConfirmationType.MarketSellTransaction or ConfirmationType.WebKey
                    or ConfirmationType.Recovery).ToArray();

            var window = new ConfirmationsWindow(confirmations, steamGuardAccount);

            window.Show();
        }
        catch (RequestException e)
        {
            await NotificationsMessageWindow.ShowWindow($"Cannot load confirmations. {e}", _accountViewModel.OwnerWindow);
        }
        catch (Exception e)
        {
            await NotificationsMessageWindow.ShowWindow($"Cannot load confirmations, message: {e.Message}", _accountViewModel.OwnerWindow);
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
        var newSteamTime = new SimpleSteamTime();

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