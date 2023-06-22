using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;
using TradeOnSda.Windows.NotificationMessage;

namespace TradeOnSda.Views.ImportAccounts;

public class ImportAccountsViewModel : ViewModelBase
{
    private string _password = null!;
    private ICommand _commitPassword = null!;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
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

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Func<string, IWebProxy?, string, SdaSettings, Task<bool>> _addAccountFunc;
    private string _proxyString = null!;
    private bool _autoConfirm;

    public ImportAccountsViewModel(ulong steamId, string login, string maFileName,
        Func<string, IWebProxy?, string, SdaSettings, Task<bool>> addAccountFunc, SdaManager sdaManager,
        Window ownerWindow)
    {
        Login = login;
        SteamId = steamId.ToString();
        MaFileName = maFileName;
        _ownerWindow = ownerWindow;
        Password = string.Empty;
        _addAccountFunc = addAccountFunc;
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
                proxy = ParseWebProxy();
            }
            catch (Exception)
            {
                await NotificationsMessageWindow.ShowWindow("Cannot parse proxy string", _ownerWindow);
                return;
            }

            var loginResult = await _addAccountFunc(Password, proxy, ProxyString, new SdaSettings(AutoConfirm));

            if (!loginResult)
            {
                await NotificationsMessageWindow.ShowWindow("Error login in steam", _ownerWindow);
                return;
            }

            sdaManager.GlobalSettings.DefaultEnabledAutoConfirm = AutoConfirm;
            await sdaManager.SaveSettingsAsync();

            _ownerWindow.Close();
        });
    }

    private IWebProxy? ParseWebProxy()
    {
        if (string.IsNullOrWhiteSpace(ProxyString))
            return null;

        var tokens = ProxyString.Split(':');

        return tokens.Length switch
        {
            2 => new WebProxy(tokens[0], int.Parse(tokens[1])),
            4 => new WebProxy(tokens[0], int.Parse(tokens[1]))
            {
                Credentials = new NetworkCredential(tokens[2], tokens[3]),
            },
            _ => throw new Exception("Invalid proxy format")
        };
    }

    public ImportAccountsViewModel()
    {
        _ownerWindow = null!;
        Login = "TestAccountLogin";
        SteamId = 1234567890ul.ToString();
        MaFileName = "1234567890.maFile";
        Password = "";
        _addAccountFunc = null!;
    }
}