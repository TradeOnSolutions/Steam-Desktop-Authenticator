using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Responses;

public class NewBuyOrderResponse
{
    [JsonProperty("success")]
    public int Success { get; set; }
    
    [JsonProperty("buy_orderid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long BuyOrderId { get; set; }
}