using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SteamAuthentication.LogicModels;

namespace TradeOnSda.Data;

public class GlobalSteamTime : ISteamTime
{
    private readonly TimeDeferenceRestClient _timeDeferenceRestClient;

    public GlobalSteamTime(TimeDeferenceRestClient timeDeferenceRestClient)
    {
        _timeDeferenceRestClient = timeDeferenceRestClient;
        _asyncLock = new AsyncLock();
    }

    private readonly AsyncLock _asyncLock;
    private long? _timeDifference;

    private long GetCurrentClientTime() =>
        (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

    private long GetCurrentSteamTimeInternal()
    {
        if (_timeDifference == null)
            throw new Exception("SteamTime is not synchronized");

        return GetCurrentClientTime() + _timeDifference.Value;
    }

    public long GetCurrentSteamTime() => GetCurrentSteamTimeAsync(CancellationToken.None).GetAwaiter().GetResult();

    public async Task<long> GetCurrentSteamTimeAsync(CancellationToken cancellationToken)
    {
        if (_timeDifference != null)
            return GetCurrentSteamTimeInternal();

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_timeDifference != null)
                return GetCurrentSteamTimeInternal();

            var steamTime = await _timeDeferenceRestClient.GetSteamTimeAsync(cancellationToken);

            var clientTime = GetCurrentClientTime();

            _timeDifference = steamTime - clientTime;

            return GetCurrentSteamTimeInternal();
        }
    }
}