using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthentication.Logic;
using SteamAuthentication.LogicModels;

namespace TradeOnSda.Data;

public class SimpleSteamTime : ISteamTime
{
    public long GetCurrentSteamTime()
    {
        return DateTime.UtcNow.ToTimeStamp();
    }

    public Task<long> GetCurrentSteamTimeAsync(CancellationToken cancellationToken) => Task.FromResult(DateTime.UtcNow.ToTimeStamp());
}