using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TradeOnSda.Data;
using TradeOnSda.Windows.Main;

namespace TradeOnSda;

// ReSharper disable once ClassNeverInstantiated.Global
public class App : Application
{
    public AppViewModel AppViewModel { get; }
    
    public App()
    {
        AppViewModel = new AppViewModel();
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        DataContext = AppViewModel;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var sdaManager = Task.Run(async () => await SdaManager.CreateSdaManagerAsync()).GetAwaiter().GetResult();

            var window = desktop.MainWindow = new MainWindow();

            AppViewModel.MainWindow = window;

            desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow, sdaManager);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime)
            throw new NotSupportedException();

        base.OnFrameworkInitializationCompleted();
    }
}