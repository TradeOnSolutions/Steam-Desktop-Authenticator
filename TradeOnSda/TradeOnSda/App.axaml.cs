using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TradeOnSda.Data;
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
            var sdaManager = SdaManager.CreateSdaManagerAsync().GetAwaiter().GetResult();
            
            desktop.MainWindow = new MainWindow();

            desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow, sdaManager);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime)
            throw new NotSupportedException();

        base.OnFrameworkInitializationCompleted();
    }
}