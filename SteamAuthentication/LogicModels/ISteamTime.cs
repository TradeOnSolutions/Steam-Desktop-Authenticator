namespace SteamAuthentication.LogicModels;

public interface ISteamTime
{
    public long GetCurrentSteamTime();
    
    public Task<long> GetCurrentSteamTimeAsync(CancellationToken cancellationToken);
}