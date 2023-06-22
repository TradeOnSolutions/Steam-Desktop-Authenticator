using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ReactiveUI;

namespace TradeOnSda.ViewModels;

public class ViewModelBase : ReactiveObject
{
    [NotifyPropertyChangedInvocator]
    protected void RaiseAndSetIfPropertyChanged<T>(ref T obj, T value, [CallerMemberName] string? propertyName = null)
    {
        this.RaiseAndSetIfChanged(ref obj, value, propertyName);
    }

    protected void OnPropertyChanged(string propertyName)
    {
        this.RaisePropertyChanged(propertyName);
    }
}