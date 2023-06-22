using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Responses;

public class MyOrdersResponse
{
    [JsonProperty("success", Required = Required.Always)]
    public bool Success { get; set; }
    
    [JsonProperty("buy_orders")]
    public SteamBuyOrder[]? BuyOrders { get; set; }
    
    [JsonProperty("total_count", Required = Required.Always)]
    public int TotalCount { get; set; }
}

public class SteamBuyOrder
{
    [JsonProperty("appid")] public long Appid { get; set; }

    [JsonProperty("price")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long Price { get; set; }

    [JsonProperty("quantity")]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int Quantity { get; set; }

    [JsonProperty("quantity_remaining")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long QuantityRemaining { get; set; }

    [JsonProperty("buy_orderid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long BuyOrderId { get; set; }

    [JsonProperty("description", Required = Required.Always)]
    public SteamBuyOrderDescription Description { get; set; } = null!;
}

public class SteamBuyOrderDescription
{
    [JsonProperty("appid")] public long AppId { get; set; }

    [JsonProperty("classid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long ClassId { get; set; }

    [JsonProperty("instanceid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long InstanceId { get; set; }

    [JsonProperty("currency")] public int Currency { get; set; }

    [JsonProperty("tradable")] public int Tradable { get; set; }

    [JsonProperty("market_name", Required = Required.Always)]
    public string MarketName { get; set; } = null!;

    [JsonProperty("market_hash_name", Required = Required.Always)]
    public string MarketHashName { get; set; } = null!;
}