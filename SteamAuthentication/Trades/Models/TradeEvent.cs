using SteamAuthentication.Trades.Responses;

namespace SteamAuthentication.Trades.Models;

public class TradeEvent
{
    public Offer? PreviousState { get; }
    
    public Offer NewState { get; }

    public TradeEvent(Offer? previousState, Offer newState)
    {
        PreviousState = previousState;
        NewState = newState;
    }
}