using Newtonsoft.Json;
using SteamAuthentication.Trades.Models;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.Trades.Responses;

internal class SteamInventoryResponse
{
    [JsonProperty("assets", Required = Required.Always)]
    public List<InventoryItem> Assets { get; set; } = null!;

    [JsonProperty("descriptions")]
    public ItemDescription[]? Descriptions { get; set; }

    [JsonProperty("success")]
    public long Success { get; set; }

    [JsonConstructor]
    private SteamInventoryResponse()
    {
    }
}

public class InventoryItem
{
    [JsonProperty("appid", Required = Required.Always)]
    public int Appid { get; private set; }

    [JsonProperty("contextid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToIntConverter))]
    public int ContextId { get; private set; }

    [JsonProperty("assetid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long AssetId { get; private set; }

    [JsonProperty("classid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long ClassId { get; private set; }

    [JsonProperty("instanceid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long InstanceId { get; private set; }

    [JsonProperty("amount", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long Amount { get; private set; }

    [JsonConstructor]
    private InventoryItem()
    {
    }

    public ItemId GetItemId() => new(ClassId, InstanceId);
}

public class ItemDescription
{
    [JsonProperty("appid", Required = Required.Always)]
    public int Appid { get; private set; }

    [JsonProperty("classid", Required = Required.Always)]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long ClassId { get; private set; }

    [JsonProperty("instanceid")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long InstanceId { get; private set; }

    [JsonProperty("currency")]
    public long Currency { get; private set; }

    [JsonProperty("background_color")]
    public string? BackgroundColor { get; private set; }

    [JsonProperty("icon_url")]
    public string? IconUrl { get; private set; }

    [JsonProperty("icon_url_large", NullValueHandling = NullValueHandling.Ignore)]
    public string? IconUrlLarge { get; private set; }

    [JsonProperty("descriptions")]
    public TypeDescription[]? Descriptions { get; private set; }

    [JsonProperty("tradable")]
    public long Tradable { get; private set; }

    [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
    public ActionDescription[]? Actions { get; private set; }

    [JsonProperty("name")]
    public string? Name { get; private set; }

    [JsonProperty("name_color")]
    public string? NameColor { get; private set; }

    [JsonProperty("type")]
    public string? Type { get; private set; }

    [JsonProperty("market_name")]
    public string? MarketName { get; private set; }

    [JsonProperty("market_hash_name")]
    public string? MarketHashName { get; private set; }

    [JsonProperty("market_actions")]
    public ActionDescription[]? MarketActions { get; private set; }

    [JsonProperty("commodity")]
    public long Commodity { get; private set; }

    [JsonProperty("market_tradable_restriction")]
    public long MarketTradableRestriction { get; private set; }

    [JsonProperty("marketable", Required = Required.Always)]
    public long Marketable { get; private set; }

    [JsonProperty("tags")]
    public TagDescription[]? Tags { get; private set; }

    [JsonConstructor]
    private ItemDescription()
    {
    }
}

public class ActionDescription
{
    [JsonProperty("link")]
    public string? Link { get; private set; }

    [JsonProperty("name")]
    public string? Name { get; private set; }

    [JsonConstructor]
    private ActionDescription()
    {
    }
}

public class TypeDescription
{
    [JsonProperty("type")]
    public string? Type { get; private set; }

    [JsonProperty("value")]
    public string? Value { get; private set; }

    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    public string? Color { get; private set; }

    [JsonConstructor]
    private TypeDescription()
    {
    }
}

public class TagDescription
{
    [JsonProperty("category")]
    public string? Category { get; private set; }

    [JsonProperty("internal_name")]
    public string? InternalName { get; private set; }

    [JsonProperty("localized_category_name")]
    public string? LocalizedCategoryName { get; private set; }

    [JsonProperty("localized_tag_name")]
    public string? LocalizedTagName { get; private set; }

    [JsonProperty("color")]
    public string? Color { get; private set; }

    [JsonConstructor]
    private TagDescription()
    {
    }
}