using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SteamAuthentication.Models;
using TradeOnSda.Data;
using TradeOnSda.Views.ImportAccounts;

namespace TradeOnSda.Windows.ImportAccounts;

public partial class ImportAccountsWindow : Window
{
    public ImportAccountsWindow(SteamMaFile maFile, string maFileName, SdaManager sdaManager)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        DataContext =
            new ImportAccountsWindowViewModel(new ImportAccountsViewModel(maFile, maFileName, sdaManager,
                this));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static async Task CreateImportAccountWindowAsync(SteamMaFile maFile, string maFileName, Window ownerWindow,
        SdaManager sdaManager)
    {
        var window = new ImportAccountsWindow(maFile, maFileName, sdaManager);

        await window.ShowDialog(ownerWindow);
    }
}