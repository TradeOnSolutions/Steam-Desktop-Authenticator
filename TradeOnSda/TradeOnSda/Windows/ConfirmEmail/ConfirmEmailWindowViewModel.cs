using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using TradeOnSda.ViewModels;

namespace TradeOnSda.Windows.ConfirmEmail;

public class ConfirmEmailWindowViewModel : ViewModelBase
{
    public string Email { get; }
    
    public ICommand DoneCommand { get; }

    public ConfirmEmailWindowViewModel(string email, Window ownerWindow)
    {
        Email = email;

        DoneCommand = ReactiveCommand.Create(ownerWindow.Close);
    }

    public ConfirmEmailWindowViewModel()
    {
        Email = null!;
        DoneCommand = null!;
    }
}