using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using SteamAuthentication.Exceptions;
using SteamAuthentication.Logic;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Trades.Models;
using SteamAuthentication.Trades.Requests;
using SteamAuthentication.Trades.Responses;

namespace SteamAuthentication.Trades;

public class SteamAccount
{
    private readonly ILogger<SteamAccount> _logger;

    public SteamGuardAccount SteamGuardAccount { get; }

    public string SteamApiKey { get; }

    public TradesState TradesState { get; }

    public SteamAccount(SteamGuardAccount steamGuardAccount, string steamApiKey, ILogger<SteamAccount> logger)
    {
        _logger = logger;
        SteamGuardAccount = steamGuardAccount;
        SteamApiKey = steamApiKey;
        TradesState = new TradesState(steamGuardAccount.SteamTime.GetCurrentSteamTime());
    }

    #region Inventory

    public async Task<SteamInventory?> TryGetMyInventoryAsync(int appId, int contextId = 2,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(GetMyInventoryAsync(appId, contextId, cancellationToken));

    public async Task<SteamInventory> GetMyInventoryAsync(int appId, int contextId = 2,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        _logger.LogDebug("AppId: {appId}, contextId: {contextId}", appId, contextId);

        var inventory = await GetInventoryInternalAsync(
            appId,
            contextId,
            SteamGuardAccount.MaFile.Session.SteamId,
            cancellationToken);

        return inventory;
    }

    public async Task<SteamInventory?> TryGetInventoryAsync(int appId, ulong steamId, int contextId = 2,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(GetInventoryAsync(appId, steamId, contextId, cancellationToken));

    public async Task<SteamInventory> GetInventoryAsync(int appId, ulong steamId, int contextId = 2,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        _logger.LogDebug("AppId: {appId}, contextId: {contextId}", appId, contextId);

        var inventory = await GetInventoryInternalAsync(
            appId,
            contextId,
            steamId,
            cancellationToken);

        return inventory;
    }

    private DateTime? _inventoryTime;

    private long GetTimeStampForInventoryRequest()
    {
        if (_inventoryTime == null)
            _inventoryTime = DateTime.UtcNow;

        if (_inventoryTime < DateTime.UtcNow - TimeSpan.FromHours(1))
            _inventoryTime = DateTime.UtcNow;

        return _inventoryTime.Value.ToTimeStamp();
    }

    private async Task<SteamInventory> GetInventoryInternalAsync(int appId, int contextId, ulong steamId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count=5000";

        var referer = $"https://steamcommunity.com/profiles/{steamId}/inventory/";

        _logger.LogDebug("Url: {url}", url);
        _logger.LogDebug("Referer: {referer}", referer);

        var headers = new List<(string key, string value)>
        {
            ("Accept", "*/*"),
            ("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7"),
            ("Connection", "keep-alive"),
            ("Host", "steamcommunity.com"),
            ("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36"),
            ("sec-ch-ua", "\"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\", \"Google Chrome\";v=\"108\""),
            ("sec-ch-ua-mobile", "?0"),
            ("sec-ch-ua-platform", "\"Windows\""),
            ("Sec-Fetch-Dest", "empty"),
            ("Sec-Fetch-Mode", "cors"),
            ("Sec-Fetch-Site", "same-origin"),
            ("X-Requested-With", "XMLHttpRequest")
        };

        var cookies = SteamGuardAccount.MaFile.Session.CreateCookies();
        cookies.Add(new Cookie("webTradeEligibility",
                $"%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22new_device_cooldown_days%22%3A0%2C%22time_checked%22%3A{GetTimeStampForInventoryRequest()}%7D")
            { Domain = "steamcommunity.com" });

        RestResponse response;

        try
        {
            response = await SteamGuardAccount.RestClient.ExecuteGetRequestAsync(
                url,
                cookies,
                headers,
                referer,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing inventory request");
            throw;
        }

        _logger.LogRestResponse(response, "InventoryResponse");

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing inventory request", response.StatusCode, null, null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing inventory request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        SteamInventoryResponse? steamInventoryResponse;

        try
        {
            steamInventoryResponse = JsonConvert.DeserializeObject<SteamInventoryResponse>(content);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize SteamInventoryResponse, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        if (steamInventoryResponse == null || steamInventoryResponse.Success != 1)
        {
            _logger.LogError("Inventory response is not success, content: {content}", content);
            throw new RequestException("Error while executing inventory request (not success)", response.StatusCode,
                content, null);
        }

        var itemsDictionary = steamInventoryResponse.Assets
            .ToDictionary(asset => asset.AssetId);

        var descriptionsDictionary = (steamInventoryResponse.Descriptions ?? Array.Empty<ItemDescription>())
            .ToDictionary(description => new ItemId(description.ClassId, description.InstanceId));

        var inventory = new SteamInventory(itemsDictionary, descriptionsDictionary);

        return inventory;
    }

    #endregion

    #region Getting Offers

    public async Task<(bool result, Offer[] sentOffers, Offer[] receivedOffers)> TryGetSentAndReceivedTradeOffersAsync(
        long timeStamp, CancellationToken cancellationToken = default)
    {
        try
        {
            var (sentOffers, receivedOffers) = await (GetSentAndReceivedTradeOffersAsync(timeStamp, cancellationToken));

            return (true, sentOffers, receivedOffers);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing GetSentAndReceivedTradeOffers request, exception: {exception}",
                e.ToJson());
            return (false, Array.Empty<Offer>(), Array.Empty<Offer>());
        }
    }

    public async Task<(Offer[] sentOffers, Offer[] receivedOffers)> GetSentAndReceivedTradeOffersAsync(long timeStamp,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        var response = await GetTradeOffersAsync(
            true,
            true,
            false,
            true,
            false,
            timeStamp,
            "en_us",
            cancellationToken);

        return (response.TradeOffersSent ?? Array.Empty<Offer>(), response.TradeOffersReceived ?? Array.Empty<Offer>());
    }

    public async Task<Offer[]?> TryGetSentOffersAsync(long timeStamp, CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(GetSentOffersAsync(timeStamp, cancellationToken));

    public async Task<Offer[]> GetSentOffersAsync(long timeStamp, CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        var response = await GetTradeOffersAsync(
            true,
            true,
            false,
            true,
            false,
            timeStamp,
            "en_us",
            cancellationToken);

        return response.TradeOffersSent ?? Array.Empty<Offer>();
    }

    public async Task<Offer[]?>
        TryGetReceivedOffersAsync(long timeStamp, CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(GetReceivedOffersAsync(timeStamp, cancellationToken));

    public async Task<Offer[]> GetReceivedOffersAsync(long timeStamp, CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        var response = await GetTradeOffersAsync(
            true,
            true,
            false,
            true,
            false,
            timeStamp,
            "en_us",
            cancellationToken);

        return response.TradeOffersReceived ?? Array.Empty<Offer>();
    }

    private async Task<OffersResponse> GetTradeOffersAsync(bool getSentOffers, bool getReceivedOffers,
        bool getDescriptions,
        bool activeOnly, bool historicalOnly, long timeHistoricalCutoff, string language,
        CancellationToken cancellationToken)
    {
        if (!getSentOffers && !getReceivedOffers)
        {
            _logger.LogCritical(
                "Method: GetTradeOffersAsync, getSentOffers: {getSentOffers}, getReceivedOffers: {getReceivedOffers}",
                getSentOffers, getReceivedOffers);
            throw new ArgumentException("getSentOffers and getReceivedOffers can't be both false");
        }

        var options =
            $"?key={SteamApiKey}&" +
            $"get_sent_offers={Convert.ToInt32(getSentOffers)}&" +
            $"get_received_offers={Convert.ToInt32(getReceivedOffers)}&" +
            $"get_descriptions={Convert.ToInt32(getDescriptions)}&" +
            $"language={language}&" +
            $"active_only={Convert.ToInt32(activeOnly)}&" +
            $"historical_only={Convert.ToInt32(historicalOnly)}&" +
            $"time_historical_cutoff={timeHistoricalCutoff}";

        _logger.LogDebug("Options: {options}", options);

        var url = string.Format(Endpoints.SteamIEconServiceBaseUrl,
            "GetTradeOffers", "v1", options);

        RestResponse response;

        try
        {
            response = await SteamGuardAccount.RestClient.ExecuteGetRequestAsync(url,
                SteamGuardAccount.MaFile.Session.CreateCookies(),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing offers request, exception: {exception}", e.ToJson());
            throw;
        }

        _logger.LogRestResponse(response, "OffersRequest");

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing offers request", response.StatusCode, null, null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing offers request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);


        OffersWrapperResponse<OffersResponse>? wrapper;

        try
        {
            wrapper = JsonConvert.DeserializeObject<OffersWrapperResponse<OffersResponse>>(content);

            if (wrapper == null)
                throw new RequestException("Error while executing offers request (result is null)", response.StatusCode,
                    content, null);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error deserialize OffersWrapperResponse<OffersResponse>, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        var offersResponse = wrapper.Response;

        _logger.LogDebug("Ok");

        return offersResponse;
    }

    #endregion

    #region Offers Working

    public async Task<bool>
        TryAcceptTradeOfferAsync(ulong tradeOfferId, CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(AcceptTradeOfferAsync(tradeOfferId, cancellationToken));

    public async Task AcceptTradeOfferAsync(ulong tradeOfferId, CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        const string method = "accept";
        
        await ProcessTradeOfferAsync(tradeOfferId, method, cancellationToken);
    }

    public async Task<bool>
        TryDeclineTradeOfferAsync(ulong tradeOfferId, CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(DeclineTradeOfferAsync(tradeOfferId, cancellationToken));

    public async Task DeclineTradeOfferAsync(ulong tradeOfferId, CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        const string method = "decline";

        await ProcessTradeOfferAsync(tradeOfferId, method, cancellationToken);
    }

    public async Task<bool>
        TryCancelTradeOfferAsync(ulong tradeOfferId, CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(CancelTradeOfferAsync(tradeOfferId, cancellationToken));

    public async Task CancelTradeOfferAsync(ulong tradeOfferId, CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        const string method = "cancel";

        await ProcessTradeOfferAsync(tradeOfferId, method, cancellationToken);
    }

    private async Task ProcessTradeOfferAsync(ulong tradeOfferId, string method,
        CancellationToken cancellationToken)
    {
        var options = $"?key={SteamApiKey}&tradeofferid={tradeOfferId}";

        _logger.LogDebug("Options: {options}", options);

        var data = new List<(string key, string value)>
        {
            ("sessionid", SteamGuardAccount.MaFile.Session.SessionId),
            ("tradeofferid", tradeOfferId.ToString()),
            ("serverid", "1")
        };

        var query = string.Join('&',
            data.Select(t => $"{WebUtility.UrlEncode(t.key)}={WebUtility.UrlEncode(t.value)}"));

        var url = $"https://steamcommunity.com/tradeoffer/{tradeOfferId}/{method}";
        var referer = $"https://steamcommunity.com/tradeoffer/{tradeOfferId}/";

        _logger.LogDebug("Url: {url}", url);

        RestResponse response;

        try
        {
            response = await SteamGuardAccount.RestClient.ExecutePostRequestAsync(
                url, SteamGuardAccount.MaFile.Session.CreateCookies(), null, referer, query, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing process offer request, exception: {exception}", e.ToJson());
            throw;
        }

        _logger.LogRestResponse(response, "ProcessOffer");

        string? content = null;

        if (response.RawBytes != null)
            content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing process offer request",
                response.StatusCode, content, null);

        if (content == null)
            throw new RequestException("Error while executing process offer request, raw bytes is null",
                response.StatusCode,
                content, null);

        _logger.LogDebug("TradeOffer success");
    }

    #endregion

    #region Offer Creating

    public async Task<long?> TryCreateTradeOfferAsync(IEnumerable<TradeAsset> itemsToGive,
        IEnumerable<TradeAsset> itemsToReceive, ulong partnerId, string message, string offerAccessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await CreateTradeOfferAsync(itemsToGive, itemsToReceive, partnerId, message, offerAccessToken,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<long> CreateTradeOfferAsync(IEnumerable<TradeAsset> itemsToGive,
        IEnumerable<TradeAsset> itemsToReceive,
        ulong partnerId,
        string message,
        string offerAccessToken,
        CancellationToken cancellationToken = default)
    {
        var tradeItemsRequest = new TradeStatusRequest(itemsToGive, itemsToReceive);

        var data = new List<(string key, string value)>
        {
            ("sessionid", SteamGuardAccount.MaFile.Session.SessionId),
            ("serverid", "1"),
            ("partner", partnerId.ToString()),
            ("tradeoffermessage", message),
            ("json_tradeoffer", JsonConvert.SerializeObject(tradeItemsRequest)),
            ("captcha", ""),
            ("trade_offer_create_params", $"{{\"trade_offer_access_token\":\"{offerAccessToken}\"}}")
        };

        var referer = $"https://steamcommunity.com/tradeoffer/new/?partner={partnerId}";

        RestResponse response;

        var cookies = SteamGuardAccount.MaFile.Session.CreateCookies();
        cookies.Add(new Cookie("webTradeEligibility",
                $"%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22new_device_cooldown_days%22%3A0%2C%22time_checked%22%3A{GetTimeStampForInventoryRequest()}%7D")
            { Domain = "steamcommunity.com" });

        try
        {
            response = await SteamGuardAccount.RestClient.ExecutePostRequestAsync(
                "https://steamcommunity.com/tradeoffer/new/send",
                cookies,
                null,
                referer,
                string.Join('&', data.Select(t => $"{WebUtility.UrlEncode(t.key)}={WebUtility.UrlEncode(t.value)}")),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing create offer request, exception: {exception}", e.ToJson());
            throw;
        }

        _logger.LogRestResponse(response, "CreateOfferRequest");

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing create offer request", response.StatusCode, null, null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing create offer request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        NewOfferResponse? offerResponse;

        try
        {
            offerResponse = JsonConvert.DeserializeObject<NewOfferResponse>(content);

            if (offerResponse == null)
                throw new RequestException("Deserialize error, result is null", response.StatusCode, content, null);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize NewOfferResponse, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        var tradeOfferId = offerResponse.TradeOfferId;

        if (tradeOfferId == 0)
            throw new RequestException("TradeOfferId equals zero, error", response.StatusCode, content, null);

        return tradeOfferId;
    }

    #endregion

    #region Offers Observing

    public TradeManager CreateTradeManager(TimeSpan updateInterval) => new(this, updateInterval);

    #endregion

    #region Orders Creating

    public async Task<long> CreateBuyOrderAsync(int appId, int currency, string marketHashName, decimal price,
        int count,
        CancellationToken cancellationToken = default)
    {
        var roundedPrice = (int)Math.Round(price * 100m);

        var data = new Dictionary<string, string>
        {
            { "sessionid", SteamGuardAccount.MaFile.Session.SessionId },
            { "appid", appId.ToString() },
            { "currency", currency.ToString() },
            { "market_hash_name", marketHashName },
            { "price_total", (roundedPrice * count).ToString() },
            { "quantity", count.ToString() },
        };

        data["billing_state"] = "";
        data["save_my_address"] = "0";

        const string url = "https://steamcommunity.com/market/createbuyorder/";

        var cookies = SteamGuardAccount.MaFile.Session.CreateCookies();

        cookies.Add(new Cookie("webTradeEligibility",
                $"%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22new_device_cooldown_days%22%3A0%2C%22time_checked%22%3A{GetTimeStampForInventoryRequest()}%7D")
            { Domain = "steamcommunity.com" });

        var referer = $"https://steamcommunity.com/market/listings/{appId}/{Uri.EscapeDataString(marketHashName)}";

        var response = await SteamGuardAccount.RestClient.ExecutePostRequestAsync(
            url,
            cookies,
            null,
            referer,
            string.Join('&', data.Select(t => $"{WebUtility.UrlEncode(t.Key)}={WebUtility.UrlEncode(t.Value)}")),
            cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing create buy order request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing create buy order request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        NewBuyOrderResponse? orderResponse;

        try
        {
            orderResponse = JsonConvert.DeserializeObject<NewBuyOrderResponse>(content);

            if (orderResponse == null)
                throw new RequestException("Deserialize error, result is null", response.StatusCode, content, null);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize NewBuyOrderResponse, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        if (orderResponse.Success != 1)
            throw new RequestException("BuyOrder not created. Succes != 1", response.StatusCode, content, null);

        var orderId = orderResponse.BuyOrderId;

        if (orderId == 0)
            throw new RequestException("BuyOrderId equals zero, error", response.StatusCode, content, null);

        return orderId;
    }

    public async Task DeleteBuyOrderAsync(long buyOrderId, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, string>
        {
            { "sessionid", SteamGuardAccount.MaFile.Session.SessionId },
            { "buy_orderid", buyOrderId.ToString() }
        };

        var url = "https://steamcommunity.com/market/cancelbuyorder/";
        var referer = "https://steamcommunity.com/market/";

        var cookies = SteamGuardAccount.MaFile.Session.CreateCookies();

        cookies.Add(new Cookie("webTradeEligibility",
                $"%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22new_device_cooldown_days%22%3A0%2C%22time_checked%22%3A{GetTimeStampForInventoryRequest()}%7D")
            { Domain = "steamcommunity.com" });

        var response = await SteamGuardAccount.RestClient.ExecutePostRequestAsync(
            url,
            cookies,
            null,
            referer,
            string.Join('&', data.Select(t => $"{WebUtility.UrlEncode(t.Key)}={WebUtility.UrlEncode(t.Value)}")),
            cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing delete buy order request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing delete buy order request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        DeleteBuyOrderResponse? deleteBuyOrderResponse;

        try
        {
            deleteBuyOrderResponse = JsonConvert.DeserializeObject<DeleteBuyOrderResponse>(content);

            if (deleteBuyOrderResponse == null)
                throw new RequestException("Deserialize error, result is null", response.StatusCode, content, null);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize DeleteBuyOrderResponse, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        if (deleteBuyOrderResponse.Success != 1)
            throw new RequestException("BuyOrder not deleted, success != 1", response.StatusCode, content, null);
    }

    public async Task<OrderListResult<SteamBuyOrder>> GetBuyOrdersAsync(int start,
        CancellationToken cancellationToken = default)
    {
        var orders = await GetOrdersInternalAsync(start, cancellationToken);

        var ordersBuyOrders = orders.BuyOrders;

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (ordersBuyOrders == null)
            return new OrderListResult<SteamBuyOrder>(orders.TotalCount, Array.Empty<SteamBuyOrder>());

        return new OrderListResult<SteamBuyOrder>(orders.TotalCount, ordersBuyOrders);
    }

    private async Task<MyOrdersResponse> GetOrdersInternalAsync(int start,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://steamcommunity.com/market/mylistings?start={start}&count=100&norender=1";

        var referer = "https://steamcommunity.com/market/";

        var cookies = SteamGuardAccount.MaFile.Session.CreateCookies();

        cookies.Add(new Cookie("webTradeEligibility",
                $"%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22new_device_cooldown_days%22%3A0%2C%22time_checked%22%3A{GetTimeStampForInventoryRequest()}%7D")
            { Domain = "steamcommunity.com" });

        var response = await SteamGuardAccount.RestClient.ExecuteGetRequestAsync(
            url,
            cookies,
            null,
            referer,
            cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing getOrdersInternal request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing getOrdersInternal request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        MyOrdersResponse? myOrdersResponse;

        try
        {
            myOrdersResponse = JsonConvert.DeserializeObject<MyOrdersResponse>(content);

            if (myOrdersResponse == null)
                throw new RequestException("Deserialize error, result is null", response.StatusCode, content, null);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize MyOrdersResponse, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        if (!myOrdersResponse.Success)
            throw new RequestException("MyOrdersResponse, success != 1", response.StatusCode, content, null);

        return myOrdersResponse;
    }

    #endregion

    #region History

    public async Task<OffersHistoryResponse?> TryGetOffersHistoryAsync(int maxCount,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(GetOffersHistoryAsync(maxCount, cancellationToken));
    
    public async Task<OffersHistoryResponse> GetOffersHistoryAsync(int maxCount,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        _logger.LogDebug("maxCount: {maxCount}", maxCount);

        var options = $"?key={SteamApiKey}&max_trades={maxCount}&get_descriptions={true}";

        var url = string.Format(Endpoints.SteamIEconServiceBaseUrl, "GetTradeHistory", "v1", options);

        var cookies = SteamGuardAccount.MaFile.Session.CreateCookies();

        RestResponse response;

        try
        {
            response = await SteamGuardAccount.RestClient.ExecuteGetRequestAsync(
                url,
                cookies,
                null,
                "",
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing inventory request");
            throw;
        }

        _logger.LogRestResponse(response, "OffersHistory");

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing GetOffersHistory request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing GetOffersHistory request, raw bytes is null",
                response.StatusCode,
                null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        OffersHistoryResponseWrapper? offersHistoryResponseWrapper;

        try
        {
            offersHistoryResponseWrapper = JsonConvert.DeserializeObject<OffersHistoryResponseWrapper>(content);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error deserialize OffersHistoryResponseWrapper, content: {content}, exception: {exception}",
                content, e.ToJson());
            throw;
        }

        if (offersHistoryResponseWrapper == null)
        {
            _logger.LogError("GetOffersHistory response is not success, content: {content}", content);
            throw new RequestException("Error while executing GetOffersHistory request (not success)",
                response.StatusCode,
                content, null);
        }

        var result = offersHistoryResponseWrapper.Response;

        return result;
    }

    #endregion

    public override string ToString() =>
        $"TradeAccount, steamId: {SteamGuardAccount.MaFile.Session.SteamId}, steamApiKey: {SteamApiKey}";
}