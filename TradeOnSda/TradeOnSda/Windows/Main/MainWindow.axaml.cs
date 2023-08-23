using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TradeOnSda.Windows.Main;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        Closing += (_, args) =>
        {
            // if (args.IsProgrammatic)
            //     return;
            
            args.Cancel = true;
            Hide();
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}