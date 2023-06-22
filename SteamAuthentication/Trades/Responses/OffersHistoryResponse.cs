using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Responses;

internal class OffersHistoryResponseWrapper
{
    [JsonProperty("response", Required = Required.Always)]
    public OffersHistoryResponse Response { get; set; } = null!;
}

public class OffersHistoryResponse
{
    [JsonProperty("more")] public bool More { get; set; }

    [JsonProperty("trades", Required = Required.Always)]
    public OfferHistory[] Trades { get; set; } = null!;
}

public class OfferHistory
{
    [JsonProperty("tradeid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long Id { get; set; }

    [JsonProperty("steamid_other", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong SteamIdOther { get; set; }

    [JsonProperty("time_init", Required = Required.Always)]
    public long TimeInit { get; set; }

    [JsonProperty("status", Required = Required.Always)]
    public int Status { get; set; }

    [JsonProperty("assets_received", Required = Required.AllowNull)]
    public OfferHistoryAsset[]? AssetsReceived { get; set; }

    [JsonProperty("assets_given", Required = Required.AllowNull)]
    public OfferHistoryAsset[]? AssetsGiven { get; set; }
}

public class OfferHistoryAsset
{
    [JsonProperty("appid")] public int Appid { get; set; }

    [JsonProperty("contextid")]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int ContextId { get; set; }

    [JsonProperty("assetid")]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong AssetId { get; set; }

    [JsonProperty("amount")]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int Amount { get; set; }

    [JsonProperty("classid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long ClassId { get; set; }

    [JsonProperty("instanceid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long InstanceId { get; set; }

    [JsonProperty("new_assetid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong NewAssetId { get; set; }

    [JsonProperty("new_contextid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int NewContextId { get; set; }
}