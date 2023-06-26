using System;
using Newtonsoft.Json;

namespace TradeOnSda.Data;

public class SavedSdaDto
{
    public ulong SteamId { get; set; }

    public string Password { get; set; }

    public string? ProxyString { get; set; }

    public bool AutoConfirm { get; set; }
    
    public TimeSpan AutoConfirmDelay { get; set; } 

    public SavedSdaDto(ulong steamId, string password, string? proxyString, bool autoConfirm, TimeSpan autoConfirmDelay)
    {
        SteamId = steamId;
        Password = password;
        ProxyString = proxyString;
        AutoConfirm = autoConfirm;
        AutoConfirmDelay = autoConfirmDelay;
    }

    [JsonConstructor]
    public SavedSdaDto()
    {
        SteamId = 0;
        Password = null!;
        ProxyString = null;
        AutoConfirm = false;
        AutoConfirmDelay = TimeSpan.FromSeconds(60);
    }
}