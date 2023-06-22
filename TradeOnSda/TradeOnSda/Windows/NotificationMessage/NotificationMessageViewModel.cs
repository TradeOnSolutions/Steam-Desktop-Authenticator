using TradeOnSda.ViewModels;

namespace TradeOnSda.Windows.NotificationMessage;

public class NotificationMessageViewModel : ViewModelBase
{
    private string _message = null!;

    public string Message
    {
        get => _message;
        set => RaiseAndSetIfPropertyChanged(ref _message, value);
    }

    public NotificationMessageViewModel()
    {
        Message = "";
    }

    public static NotificationMessageViewModel CreateViewModel(string message) =>
        new()
        {
            Message = message
        };
}