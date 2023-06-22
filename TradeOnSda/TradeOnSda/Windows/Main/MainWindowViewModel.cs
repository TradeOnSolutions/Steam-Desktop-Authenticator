using Avalonia.Controls;
using TradeOnSda.ViewModels;
using TradeOnSda.Views.Main;

namespace TradeOnSda.Windows.Main;

public class MainWindowViewModel : ViewModelBase
{
    public MainViewModel MainViewModel { get; }

    public MainWindowViewModel(Window ownerWindow)
    {
        MainViewModel = new MainViewModel(ownerWindow);
    }

    public MainWindowViewModel()
    {
        MainViewModel = new MainViewModel();
    }
}