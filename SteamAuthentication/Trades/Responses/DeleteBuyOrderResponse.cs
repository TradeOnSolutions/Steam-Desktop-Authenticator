using Newtonsoft.Json;

namespace SteamAuthentication.Trades.Responses;

public class DeleteBuyOrderResponse
{
    [JsonProperty("success")]
    public int Success { get; set; }
}