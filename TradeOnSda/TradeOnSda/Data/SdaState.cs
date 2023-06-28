using TradeOnSda.ViewModels;

namespace TradeOnSda.Data;

public class SdaState : ViewModelBase
{
    private ProxyState _proxyState;

    public ProxyState ProxyState
    {
        get => _proxyState;
        set => RaiseAndSetIfPropertyChanged(ref _proxyState, value);
    }

    public SdaState()
    {
        ProxyState = ProxyState.Unknown;
    }
}

public enum ProxyState
{
    Unknown,
    Ok,
    Error,
}