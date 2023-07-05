using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using SteamAuthentication.Exceptions;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;
using TradeOnSda.Windows.NotificationMessage;

namespace TradeOnSda.Views.ImportAccounts;

public class ImportAccountsViewModel : ViewModelBase
{
    private string _password = null!;
    private ICommand _commitPassword = null!;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly SteamMaFile _maFile;
    private readonly SdaManager _sdaManager;
    private readonly Window _ownerWindow;

    public string Login { get; }

    public string SteamId { get; }

    public string MaFileName { get; }

    public string Password
    {
        get => _password;
        set => RaiseAndSetIfPropertyChanged(ref _password, value);
    }

    public string ProxyString
    {
        get => _proxyString;
        set => RaiseAndSetIfPropertyChanged(ref _proxyString, value);
    }

    public bool AutoConfirm
    {
        get => _autoConfirm;
        set => RaiseAndSetIfPropertyChanged(ref _autoConfirm, value);
    }

    public ICommand CommitPassword
    {
        get => _commitPassword;
        set => RaiseAndSetIfPropertyChanged(ref _commitPassword, value);
    }

    private string _proxyString = null!;
    private bool _autoConfirm;

    public ImportAccountsViewModel(SteamMaFile maFile, string maFileName, SdaManager sdaManager,
        Window ownerWindow)
    {
        Login = maFile.AccountName;
        SteamId = maFile.Session.SteamId.ToString();
        MaFileName = maFileName;
        _maFile = maFile;
        _sdaManager = sdaManager;
        _ownerWindow = ownerWindow;
        Password = string.Empty;
        ProxyString = string.Empty;
        AutoConfirm = sdaManager.GlobalSettings.DefaultEnabledAutoConfirm;

        CommitPassword = ReactiveCommand.CreateFromTask(async () =>
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                await NotificationsMessageWindow.ShowWindow("Password is empty", _ownerWindow);
                return;
            }

            IWebProxy? proxy;

            try
            {
                proxy = ProxyLogic.ParseWebProxy(ProxyString);
            }
            catch (Exception)
            {
                await NotificationsMessageWindow.ShowWindow("Cannot parse proxy string", _ownerWindow);
                return;
            }


            var loginResult = await AddAccountAsync(proxy, new SdaSettings(AutoConfirm, TimeSpan.FromSeconds(60)));

            if (!loginResult)
                return;

            sdaManager.GlobalSettings.DefaultEnabledAutoConfirm = AutoConfirm;
            await sdaManager.SaveGlobalSettingsAsync();

            _ownerWindow.Close();
        });
    }

    private async Task<bool> AddAccountAsync(IWebProxy? proxy, SdaSettings sdaSettings)
    {
        try
        {
            var steamTime = new SimpleSteamTime();

            var maFileCredentials =
                new MaFileCredentials(proxy, ProxyString, Password);

            var sda = new SteamGuardAccount(_maFile,
                new SteamRestClient(proxy),
                steamTime,
                NullLogger<SteamGuardAccount>.Instance);

            var loginAgainResult = await sda.LoginAgainAsync(Login, Password);

            if (loginAgainResult != null)
            {
                await NotificationsMessageWindow.ShowWindow($"Error login in steam. Message: {loginAgainResult}",
                    _ownerWindow);
                return false;
            }

            await _sdaManager.AddAccountAsync(sda, maFileCredentials, sdaSettings);

            return true;
        }
        catch (RequestException e)
        {
            await NotificationsMessageWindow.ShowWindow(
                $"{e.Message}, statusCode: {e.HttpStatusCode}, Content: {e.Content}", _ownerWindow);
            return false;
        }
        catch (Exception e)
        {
            await NotificationsMessageWindow.ShowWindow(e.Message, _ownerWindow);
            return false;
        }
    }

    public ImportAccountsViewModel()
    {
        _maFile = null!;
        _sdaManager = null!;
        _ownerWindow = null!;
        Login = "TestAccountLogin";
        SteamId = 1234567890ul.ToString();
        MaFileName = "1234567890.maFile";
        Password = "";
    }
}