using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TradeOnSda.Windows.ConfirmEmail;

public partial class ConfirmEmailWindow : Window
{
    public ConfirmEmailWindow(string email)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        DataContext = new ConfirmEmailWindowViewModel(email, this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static async Task ShowWindow(string email, Window ownerWindow) => 
        await new ConfirmEmailWindow(email).ShowDialog(ownerWindow);
}