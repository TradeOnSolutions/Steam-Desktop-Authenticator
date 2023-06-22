using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TradeOnSda.Windows.Main;

namespace TradeOnSda;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime)
            throw new NotSupportedException();

        base.OnFrameworkInitializationCompleted();
    }
}