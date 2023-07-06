using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SteamAuthentication.LogicModels;
using TradeOnSda.Data;

namespace TradeOnSda.Windows.GuardAdded;

public partial class GuardAddedWindow : Window
{
    public GuardAddedWindow(SteamGuardAccount steamGuardAccount, MaFileCredentials credentials)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        DataContext = new GuardAddedWindowViewModel(steamGuardAccount, credentials, this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static async Task ShowWindow(SteamGuardAccount steamGuardAccount, MaFileCredentials credentials,
        Window ownerWindow) => await new GuardAddedWindow(steamGuardAccount, credentials).ShowDialog(ownerWindow);
}