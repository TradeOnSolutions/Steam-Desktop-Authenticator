using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Requests;

internal class TradeStatusRequest
{
    [JsonProperty("newversion")] public bool NewVersion { get; private set; }

    [JsonProperty("version")] public int Version { get; set; }

    [JsonProperty("me")] public TradeItemsRequest GiveItems { get; set; }

    [JsonProperty("them")] public TradeItemsRequest ReceiveItems { get; set; }

    public TradeStatusRequest(IEnumerable<TradeAsset> itemsToGive, IEnumerable<TradeAsset> itemsToReceive)
    {
        Version = 2;
        NewVersion = true;

        GiveItems = new TradeItemsRequest(itemsToGive, Array.Empty<TradeAsset>(), false);
        ReceiveItems = new TradeItemsRequest(itemsToReceive, Array.Empty<TradeAsset>(), false);
    }
}

internal class TradeItemsRequest
{
    [JsonProperty("assets")] public TradeAsset[] Assets { get; set; }

    [JsonProperty("currency")] public TradeAsset[] Currency { get; set; }

    [JsonProperty("ready")] public bool IsReady { get; set; }

    public TradeItemsRequest(IEnumerable<TradeAsset> assets, IEnumerable<TradeAsset> currency, bool isReady)
    {
        Assets = assets.ToArray();
        Currency = currency.ToArray();
        IsReady = isReady;
    }
}

public class TradeAsset : IEquatable<TradeAsset>
{
    [JsonProperty("appid")] public int AppId { get; }

    [JsonProperty("contextid")] public int ContextId { get; }

    [JsonProperty("amount")] public int Amount { get; }

    [JsonProperty("assetid"), JsonConverter(typeof(ValueStringConverter))]
    public long AssetId { get; }

    [JsonIgnore]
    // [JsonProperty("currencyid"), JsonConverter(typeof(ValueStringConverter))]
    public long CurrencyId { get; }

    public TradeAsset(int appId, int contextId, int amount, long assetId)
    {
        AppId = appId;
        ContextId = contextId;
        Amount = amount;
        AssetId = assetId;
        CurrencyId = 0;
    }

    public bool Equals(TradeAsset? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return AppId == other.AppId && ContextId == other.ContextId && Amount == other.Amount &&
               AssetId == other.AssetId && CurrencyId == other.CurrencyId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((TradeAsset)obj);
    }

    public override int GetHashCode() =>
        HashCode.Combine(AppId, ContextId, Amount, AssetId, CurrencyId);
}