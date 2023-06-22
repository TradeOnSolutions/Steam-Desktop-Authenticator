using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Responses;

#nullable disable

// public class TradeOffersSummary
// {
//     [JsonProperty("pending_received_count", Required = Required.Always)]
//     public int PendingReceivedCount { get; private set; }
//
//     [JsonProperty("new_received_count", Required = Required.Always)]
//     public int NewReceivedCount { get; private set; }
//
//     [JsonProperty("updated_received_count", Required = Required.Always)]
//     public int UpdatedReceivedCount { get; private set; }
//
//     [JsonProperty("historical_received_count", Required = Required.Always)]
//     public int HistoricalReceivedCount { get; private set; }
//
//     [JsonProperty("pending_sent_count", Required = Required.Always)]
//     public int PendingSentCount { get; private set; }
//
//     [JsonProperty("newly_accepted_sent_count", Required = Required.Always)]
//     public int NewlyAcceptedSentCount { get; private set; }
//
//     [JsonProperty("updated_sent_count", Required = Required.Always)]
//     public int UpdatedSentCount { get; private set; }
//
//     [JsonProperty("historical_sent_count", Required = Required.Always)]
//     public int HistoricalSentCount { get; private set; }
//
//     [JsonConstructor]
//     private TradeOffersSummary()
//     {
//     }
// }

internal class OffersWrapperResponse<T>
    where T : class
{
    [JsonProperty("response", Required = Required.Always)]
    public T Response { get; private set; } = null!;

    [JsonConstructor]
    private OffersWrapperResponse()
    {
    }
}

public class OfferResponse
{
    [JsonProperty("offer", Required = Required.Always)]
    public Offer Offer { get; private set; } = null!;

    [JsonProperty("descriptions")] public List<OfferAssetDescription>? Descriptions { get; private set; }
}

internal class OffersResponse
{
    [JsonProperty("trade_offers_sent")] public Offer[]? TradeOffersSent { get; private set; }

    [JsonProperty("trade_offers_received")]
    public Offer[]? TradeOffersReceived { get; private set; }

    [JsonConstructor]
    private OffersResponse()
    {
    }
}

public class Offer
{
    [JsonProperty("tradeofferid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToULongConverter))]
    public ulong TradeOfferId { get; private set; }

    [JsonProperty("accountid_other", Required = Required.Always)]
    public int AccountIdOther { get; private set; }

    [JsonProperty("message")] public string? Message { get; private set; }

    [JsonProperty("expiration_time", Required = Required.Always)]
    public int ExpirationTime { get; private set; }

    [JsonProperty("trade_offer_state", Required = Required.Always)]
    public TradeOfferState TradeOfferState { get; private set; }

    [JsonProperty("items_to_give")] public CEconAsset[]? ItemsToGive { get; private set; }

    [JsonProperty("items_to_receive")] public CEconAsset[]? ItemsToReceive { get; private set; }

    [JsonProperty("is_our_offer", Required = Required.Always)]
    public bool IsOurOffer { get; private set; }

    [JsonProperty("time_created", Required = Required.Always)]
    public int TimeCreated { get; private set; }

    [JsonProperty("time_updated", Required = Required.Always)]
    public int TimeUpdated { get; private set; }

    [JsonProperty("from_real_time_trade", Required = Required.Always)]
    public bool FromRealTimeTrade { get; private set; }

    [JsonProperty("escrow_end_date", Required = Required.Always)]
    public int EscrowEndDate { get; private set; }

    [JsonProperty("confirmation_method", Required = Required.Always)]
    public TradeOfferConfirmationMethod ConfirmationMethod { get; private set; }

    [JsonConstructor]
    private Offer()
    {
    }
}

public enum TradeOfferState
{
    TradeOfferStateInvalid = 1,
    TradeOfferStateActive = 2,
    TradeOfferStateAccepted = 3,
    TradeOfferStateCountered = 4,
    TradeOfferStateExpired = 5,
    TradeOfferStateCanceled = 6,
    TradeOfferStateDeclined = 7,
    TradeOfferStateInvalidItems = 8,
    TradeOfferStateNeedsConfirmation = 9,
    TradeOfferStateCanceledBySecondFactor = 10,
    TradeOfferStateInEscrow = 11,
    TradeOfferStateUnknown = 0,
}

public enum TradeOfferConfirmationMethod
{
    TradeOfferConfirmationMethodInvalid = 0,
    TradeOfferConfirmationMethodEmail = 1,
    TradeOfferConfirmationMethodMobileApp = 2
}

public class CEconAsset
{
    [JsonProperty("appid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int AppId { get; private set; }

    [JsonProperty("contextid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int ContextId { get; private set; }

    [JsonProperty("assetid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long AssetId { get; private set; }

    [JsonProperty("classid", Required = Required.Always)]
    public long ClassId { get; private set; }

    [JsonProperty("instanceid", Required = Required.Always)]
    public long InstanceId { get; private set; }

    [JsonProperty("amount", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int Amount { get; private set; }

    [JsonProperty("missing", Required = Required.Always)]
    public bool IsMissing { get; private set; }

    [JsonConstructor]
    private CEconAsset()
    {
    }
}

public class OfferAssetDescription
{
    [JsonProperty("appid", Required = Required.Always)]
    public int AppId { get; private set; }

    [JsonProperty("classid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long ClassId { get; private set; }

    [JsonProperty("instanceid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long InstanceId { get; private set; }

    [JsonProperty("currency", Required = Required.Always)]
    public bool IsCurrency { get; private set; }

    [JsonProperty("background_color")] public string? BackgroundColor { get; private set; }

    [JsonProperty("icon_url")] public string? IconUrl { get; private set; }

    [JsonProperty("icon_url_large")] public string? IconUrlLarge { get; private set; }

    [JsonProperty("descriptions")] public OfferDescription[]? Descriptions { get; private set; }

    [JsonProperty("tradable", Required = Required.Always)]
    public bool IsTradable { get; private set; }

    [JsonProperty("owner_actions")] public OfferOwnerAction[]? OwnerActions { get; private set; }

    [JsonProperty("name")] public string? Name { get; private set; }

    [JsonProperty("name_color")] public string? NameColor { get; private set; }

    [JsonProperty("type")] public string? Type { get; private set; }

    [JsonProperty("market_name")] public string? MarketName { get; private set; }

    [JsonProperty("market_hash_name")] public string? MarketHashName { get; private set; }

    [JsonConstructor]
    private OfferAssetDescription()
    {
    }
}

public class OfferDescription
{
    [JsonProperty("type")] public string? Type { get; set; }

    [JsonProperty("value")] public string? Value { get; set; }

    [JsonConstructor]
    private OfferDescription()
    {
    }
}

public class OfferOwnerAction
{
    [JsonProperty("link")] public string? Link { get; set; }

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonConstructor]
    private OfferOwnerAction()
    {
    }
}