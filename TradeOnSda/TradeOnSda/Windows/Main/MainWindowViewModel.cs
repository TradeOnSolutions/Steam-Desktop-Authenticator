using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using TradeOnSda.ViewModels;
using TradeOnSda.Views.Main;

namespace TradeOnSda.Windows.Main;

public class MainWindowViewModel : ViewModelBase
{
    public MainViewModel MainViewModel { get; }

    public IImage Logo { get; }
    
    public MainWindowViewModel(Window ownerWindow)
    {
        MainViewModel = new MainViewModel(ownerWindow);

        var logoSteam = AssetLoader.Open(new Uri("avares://TradeOnSda/Assets/logo.png"));
        Logo = new Bitmap(logoSteam);
    }

    public MainWindowViewModel()
    {
        Logo = null!;
        MainViewModel = new MainViewModel();
    }
}