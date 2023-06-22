using System;
using System.Net;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TradeOnSda.Views.ImportAccounts;

namespace TradeOnSda.Windows.ImportAccounts;

public partial class ImportAccountsWindow : Window
{
    public ImportAccountsWindow(ulong steamId, string login, string maFileName,
        Func<string, IWebProxy?, string, Task<bool>> addAccountFunc)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        DataContext =
            new ImportAccountsWindowViewModel(new ImportAccountsViewModel(steamId, login, maFileName, addAccountFunc,
                this));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static async Task CreateImportAccountWindowAsync(ulong steamId, string login, string maFileName,
        Func<string, IWebProxy?, string, Task<bool>> addAccountFunc, Window ownerWindow)
    {
        var window = new ImportAccountsWindow(steamId, login, maFileName, addAccountFunc);

        await window.ShowDialog(ownerWindow);
    }
}