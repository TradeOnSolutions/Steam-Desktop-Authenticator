using SteamAuthentication.Trades.Models;

namespace SteamAuthentication.Trades;

public class TradeObservable : IObservable<TradeEvent>
{
    private readonly List<IObserver<TradeEvent>> _observers;

    public TradeObservable()
    {
        _observers = new List<IObserver<TradeEvent>>();
    }

    internal void NewEvent(TradeEvent tradeEvent)
    {
        foreach (var observer in _observers) 
            observer.OnNext(tradeEvent);
    }

    public IDisposable Subscribe(IObserver<TradeEvent> observer)
    {
        if (_observers.Contains(observer))
            throw new InvalidOperationException("Observer is already exists");
        
        _observers.Add(observer);
            
        return new TradeObservableUnsubscriber(_observers, observer);
    }
}

internal class TradeObservableUnsubscriber : IDisposable
{
    private readonly List<IObserver<TradeEvent>> _observers;
    private readonly IObserver<TradeEvent> _observer;

    public TradeObservableUnsubscriber(List<IObserver<TradeEvent>> observers, IObserver<TradeEvent> observer)
    {
        _observers = observers;
        _observer = observer;
    }

    public void Dispose() => _observers.Remove(_observer);
}
