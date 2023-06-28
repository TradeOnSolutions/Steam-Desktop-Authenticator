using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Models;

internal class SdaConfirmationsResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    
    [JsonProperty("conf")]
    public SdaConfirmation[]? Confirmations { get; set; }
}

public class SdaConfirmation
{
    [JsonProperty("id", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong Id { get; set; }
    
    [JsonProperty("nonce", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong Key { get; set; }

    /// <summary>
    /// Represents either the Trade Offer ID or market transaction ID that caused this confirmation to be created.
    /// </summary>
    [JsonProperty("creator_id", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong Creator { get; set; }
    
    [JsonProperty("icon")]
    public string? Icon { get; set; }

    [JsonProperty("headline")]
    public string? Headline { get; set; }
    
    [JsonProperty("creation_time", Required = Required.Always)]
    public long CreationTimeStamp { get; set; }

    /// <summary>
    /// The type of this confirmation.
    /// </summary>
    [JsonProperty("type", Required = Required.Always)]
    public ConfirmationType ConfirmationType { get; set; }
}

public enum ConfirmationType
{
    UnknownConfirmation,
    UnknownConfirmation2,
    Trade,
    MarketSellTransaction,
    UnknownConfirmation3,
    UnknownConfirmation4,
    Recovery,
}