using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Responses;

public class NewOfferResponse   
{
    [JsonProperty("tradeofferid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    // ReSharper disable once InconsistentNaming
    public long TradeOfferId { get; set; }

    [JsonProperty("needs_mobile_confirmation")]
    public bool NeedsMobileConfirmation { get; set; }

    [JsonProperty("needs_email_confirmation")]
    public bool NeedsEmailConfirmation { get; set; }

    [JsonProperty("email_domain")]
    public string? EmailDomain { get; set; }
}