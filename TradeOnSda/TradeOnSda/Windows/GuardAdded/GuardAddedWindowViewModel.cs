using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using SteamAuthentication.LogicModels;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;

namespace TradeOnSda.Windows.GuardAdded;

public class GuardAddedWindowViewModel : ViewModelBase
{
    public SteamGuardAccount SteamGuardAccount { get; }

    public string AccountName { get; }
    
    public string? ProxyString { get; }
    
    public bool IsVisibleProxy { get; }
    
    public string RevocationCode { get; }
    
    public string SteamId { get; }

    public ICommand OkCommand { get; }
    
    public GuardAddedWindowViewModel(SteamGuardAccount steamGuardAccount, MaFileCredentials credentials, Window ownerWindow)
    {
        SteamGuardAccount = steamGuardAccount;
        
        AccountName = SteamGuardAccount.MaFile.AccountName;
        ProxyString = credentials.ProxyString;
        IsVisibleProxy = credentials.ProxyString != null;
        RevocationCode = steamGuardAccount.MaFile.RevocationCode!;
        SteamId = steamGuardAccount.MaFile.Session!.SteamId.ToString();

        OkCommand = ReactiveCommand.Create(ownerWindow.Close);
    }

    public GuardAddedWindowViewModel()
    {
        SteamGuardAccount = null!;
        AccountName = null!;
        ProxyString = null;
        IsVisibleProxy = false;
        RevocationCode = null!;
        SteamId = null!;
        OkCommand = null!;
    }
}