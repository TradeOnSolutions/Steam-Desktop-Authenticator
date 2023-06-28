using System;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using TradeOnSda.ViewModels;

namespace TradeOnSda;

public class AppViewModel : ViewModelBase
{
    public Window? MainWindow { get; set; }
    
    public ICommand OpenCommand { get; }
    
    public ICommand ExitCommand { get; }

    public AppViewModel()
    {
        OpenCommand = ReactiveCommand.Create(() =>
        {
            MainWindow?.Show();
        });

        ExitCommand = ReactiveCommand.Create(() =>
        {
            MainWindow?.Close();
        });
    }
}