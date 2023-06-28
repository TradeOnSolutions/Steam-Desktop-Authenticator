using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using SteamAuthentication.Exceptions;
using SteamAuthentication.Logic;
using SteamAuthentication.Models;
using SteamAuthentication.Responses;

namespace SteamAuthentication.LogicModels;

public class SteamGuardAccount
{
    public ISteamTime SteamTime { get; }

    private readonly ILogger<SteamGuardAccount> _logger;

    public SteamMaFile MaFile { get; private set; }

    public SteamRestClient RestClient { get; }

    public SteamGuardAccount(SteamMaFile maFile, SteamRestClient restClient, ISteamTime steamTime,
        ILogger<SteamGuardAccount> logger)
    {
        SteamTime = steamTime;
        _logger = logger;
        MaFile = maFile;
        RestClient = restClient;
    }

    public async Task<string?>
        TryGenerateSteamGuardCodeForTimeStampAsync(CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(GenerateSteamGuardCodeForTimeStampAsync(cancellationToken));

    public async Task<string> GenerateSteamGuardCodeForTimeStampAsync(CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        long timeStamp;

        try
        {
            timeStamp = await SteamTime.GetCurrentSteamTimeAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Exception while get steamTime: {exception}", e.ToJson());
            throw;
        }

        try
        {
            var code = SteamGuardCodeGenerating.GenerateSteamGuardCode(MaFile.SharedSecret, timeStamp, _logger);

            _logger.LogDebug("Steam guard got, code: {code}", code);

            if (code == null)
                throw new Exception("Steam guard code is null");

            return code;
        }
        catch (Exception e)
        {
            _logger.LogError("Exception while generating steam guard code: {exception}", e);
            throw;
        }
    }

    public async Task<SdaConfirmation[]?> TryFetchConfirmationAsync(CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(FetchConfirmationAsync(cancellationToken));

    public async Task<SdaConfirmation[]> FetchConfirmationAsync(CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        var url = SdaConfirmationsLogic.GenerateConfirmationUrl(
            await SteamTime.GetCurrentSteamTimeAsync(cancellationToken),
            MaFile.DeviceId,
            MaFile.IdentitySecret,
            MaFile.Session.SteamId,
            "conf",
            _logger);

        _logger.LogDebug("Created url: {url}", url);

        var cookies = MaFile.Session.CreateCookies();

        _logger.LogDebug("Created cookies: {cookies}", cookies.ToJson());

        RestResponse response;

        try
        {
            response = await RestClient.ExecuteGetRequestAsync(url, cookies, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Exception while executing request, exception: {exception}", e.ToJson());
            throw new RequestException("Exception while executing request", null, null, e);
        }

        _logger.LogRestResponse(response);

        if (!response.IsSuccessful)
            throw new RequestException("Response is not successful", response.StatusCode, response.Content, null);

        if (response.RawBytes == null)
            throw new RequestException("RawBytes is null", response.StatusCode, null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        try
        {
            var confirmationsResponse = JsonConvert.DeserializeObject<SdaConfirmationsResponse>(content);

            if (confirmationsResponse == null || !confirmationsResponse.Success)
                throw new RequestException("Response parse result is null or not success", response.StatusCode, content,
                    null);

            if (confirmationsResponse.Confirmations == null)
                throw new RequestException("Confirmations not found", response.StatusCode, content, null);

            return confirmationsResponse.Confirmations;
        }
        catch (Exception e)
        {
            _logger.LogError("Error parse confirmations, exception: {exception}", e.ToJson());
            throw;
        }
    }

    #region AcceptDenyConfirmations

    public async Task<bool> TryAcceptConfirmationAsync(SdaConfirmation confirmation,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(AcceptConfirmationAsync(confirmation, cancellationToken));

    public async Task AcceptConfirmationAsync(SdaConfirmation confirmation,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        await ProcessConfirmationAsync(confirmation, "allow",
            await SteamTime.GetCurrentSteamTimeAsync(cancellationToken), cancellationToken);
    }

    public async Task<bool> TryAcceptConfirmationsAsync(SdaConfirmation[] confirmations,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(AcceptConfirmationsAsync(confirmations, cancellationToken));

    public async Task AcceptConfirmationsAsync(SdaConfirmation[] confirmations,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        await ProcessConfirmationsAsync(confirmations, "allow",
            await SteamTime.GetCurrentSteamTimeAsync(cancellationToken), cancellationToken);
    }

    public async Task<bool> TryDenyConfirmationAsync(SdaConfirmation confirmation,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(DenyConfirmationAsync(confirmation, cancellationToken));

    public async Task DenyConfirmationAsync(SdaConfirmation confirmation, CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        await ProcessConfirmationAsync(confirmation, "cancel",
            await SteamTime.GetCurrentSteamTimeAsync(cancellationToken),
            cancellationToken);
    }
    
    public async Task<bool> TryDenyConfirmationsAsync(SdaConfirmation[] confirmations,
        CancellationToken cancellationToken = default) =>
        await TryHelpers.TryAsync(DenyConfirmationsAsync(confirmations, cancellationToken));

    public async Task DenyConfirmationsAsync(SdaConfirmation[] confirmations,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        await ProcessConfirmationsAsync(confirmations, "cancel",
            await SteamTime.GetCurrentSteamTimeAsync(cancellationToken), cancellationToken);
    }

    private async Task ProcessConfirmationsAsync(SdaConfirmation[] confirmations, string tag, long timestamp,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Start processing confirmations, count: {confirmationsCount}, tag: {tag}, timestamp: {timeStamp}",
            confirmations.Length, tag, timestamp);

        var url = Endpoints.SteamCommunityUrl + "/mobileconf/multiajaxop";
        
        var queryString = new StringBuilder();

        queryString.Append("op=" + tag + "&");

        queryString.Append(SdaConfirmationsLogic.GenerateConfirmationQueryParams(tag, MaFile.DeviceId,
            MaFile.IdentitySecret,
            MaFile.Session.SteamId,
            timestamp,
            _logger));

        foreach (var confirmation in confirmations)
            queryString.Append("&cid[]=" + confirmation.Id + "&ck[]=" + confirmation.Key);

        _logger.LogDebug("Result url: {url}", url);

        var cookies = MaFile.Session.CreateCookies();

        _logger.LogDebug("Cookies: {cookies}", cookies.ToJson());

        RestResponse response;

        try
        {
            response = await RestClient.ExecutePostRequestAsync(url, cookies, null, queryString.ToString(), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing request, exception: {exception}", e.ToJson());
            throw new RequestException("Exception while executing request", null, null, e);
        }

        _logger.LogRestResponse(response);

        if (!response.IsSuccessful)
            throw new RequestException("Response is not successful", response.StatusCode, response.Content, null);

        if (response.RawBytes == null)
            throw new RequestException("RawBytes is null", response.StatusCode, null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        try
        {
            var processConfirmationResponse = JsonConvert.DeserializeObject<ProcessConfirmationResponse>(content);

            if (processConfirmationResponse == null)
                throw new RequestException("Response parse result is null", response.StatusCode, content, null);

            if (!processConfirmationResponse.Success)
                throw new Exception("ConfirmationResponse is false");
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deserializing content, content: {content}, exception: {exception}", content,
                e.ToJson());
            throw;
        }
    }

    private async Task ProcessConfirmationAsync(
        SdaConfirmation confirmation,
        string tag,
        long timestamp,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Start processing confirmation, id: {confirmationId}, tag: {tag}, timestamp: {timeStamp}",
            confirmation.Id, tag, timestamp);

        var url = Endpoints.SteamCommunityUrl + "/mobileconf/ajaxop";

        var queryString = "?op=" + tag + "&";

        queryString += SdaConfirmationsLogic.GenerateConfirmationQueryParams(tag, MaFile.DeviceId,
            MaFile.IdentitySecret,
            MaFile.Session.SteamId,
            timestamp,
            _logger);

        queryString += "&cid=" + confirmation.Id + "&ck=" + confirmation.Key;

        url += queryString;

        _logger.LogDebug("Result url: {url}", url);

        var cookies = MaFile.Session.CreateCookies();

        _logger.LogDebug("Cookies: {cookies}", cookies.ToJson());

        RestResponse response;

        try
        {
            response = await RestClient.ExecuteGetRequestAsync(url, cookies, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing request, exception: {exception}", e.ToJson());
            throw new RequestException("Exception while executing request", null, null, e);
        }

        _logger.LogRestResponse(response);

        if (!response.IsSuccessful)
            throw new RequestException("Response is not successful", response.StatusCode, response.Content, null);

        if (response.RawBytes == null)
            throw new RequestException("RawBytes is null", response.StatusCode, null, null);

        var content = await GZipDecoding.DecodeGZipAsync(response.RawBytes, _logger, cancellationToken);

        try
        {
            var processConfirmationResponse = JsonConvert.DeserializeObject<ProcessConfirmationResponse>(content);

            if (processConfirmationResponse == null)
                throw new RequestException("Response parse result is null", response.StatusCode, content, null);

            if (!processConfirmationResponse.Success)
                throw new Exception("ConfirmationResponse is false");
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deserializing content, content: {content}, exception: {exception}", content,
                e.ToJson());
            throw;
        }
    }

    #endregion

    public async Task<string?> TryLoginAgainAsync(string username, string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await LoginAgainAsync(username, password, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return "unknown";
        }
    }

    public async Task<string?> LoginAgainAsync(string username, string password,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.CreateScopeForMethod(this);

        var cookies = new CookieContainer();

        cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
        cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));
        cookies.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));

        var headers = new List<(string name, string value)>
            { ("X-Requested-With", "com.valvesoftware.android.steam.community") };

        var referer = Endpoints.SteamCommunityUrl +
                      "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";

        var cookiesUrl =
            "https://steamcommunity.com/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";

        RestResponse cookiesResponse;

        try
        {
            cookiesResponse = await RestClient.ExecuteGetRequestAsync(cookiesUrl, cookies, headers, referer,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing cookies request, exception: {exception}", e.ToJson());
            throw;
        }

        _logger.LogRestResponse(cookiesResponse, "CookiesResponse");

        if (!cookiesResponse.IsSuccessful)
            throw new RequestException("Error while executing cookies request", cookiesResponse.StatusCode, null, null);

        _logger.LogDebug("New cookies: {cookies}", cookies.ToJson());

        var rsaBody =
            $"{WebUtility.UrlEncode("donotcache")}={await SteamTime.GetCurrentSteamTimeAsync(cancellationToken)}&username={username}";

        RestResponse rsaResponse;

        try
        {
            rsaResponse = await RestClient.ExecutePostRequestAsync(
                Endpoints.SteamCommunityUrl + "/login/getrsakey",
                cookies,
                headers,
                referer,
                rsaBody, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing rsa request, exception: {exception}", e.ToJson());
            throw;
        }

        _logger.LogRestResponse(rsaResponse, "RsaResponse");

        if (!rsaResponse.IsSuccessful)
            throw new RequestException("Error while executing rsa request", rsaResponse.StatusCode, null, null);

        if (rsaResponse.RawBytes == null)
            throw new RequestException("Error while executing rsa request, raw bytes is null", rsaResponse.StatusCode,
                null, null);

        var rsaContent = await GZipDecoding.DecodeGZipAsync(rsaResponse.RawBytes, _logger, cancellationToken);

        if (rsaContent.ToLower().Contains("<BODY>\nAn error occurred while processing your request.".ToLower()))
        {
            _logger.LogError("Error while executing rsa request, response content is bad");

            throw new RequestException("Error while executing rsa request, response content is bad",
                rsaResponse.StatusCode, rsaContent, null);
        }

        RsaResponse? rsa;

        try
        {
            rsa = JsonConvert.DeserializeObject<RsaResponse>(rsaContent);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize rsa response, content: {content}, exception: {exception}", rsaContent,
                e.ToJson());
            throw;
        }

        if (rsa == null || !rsa.Success)
        {
            _logger.LogError("Rsa result is not success, content: {content}", rsaContent);
            throw new RequestException("Error while executing rsa request, success = false", rsaResponse.StatusCode,
                rsaContent, null);
        }

        await Task.Delay(350, cancellationToken);

        using var rsaEncryptor = new RSACryptoServiceProvider();

        var passwordBytes = Encoding.ASCII.GetBytes(password);
        var rsaParameters = rsaEncryptor.ExportParameters(false);
        rsaParameters.Exponent = RsaUtility.HexStringToByteArray(rsa.Exponent);
        rsaParameters.Modulus = RsaUtility.HexStringToByteArray(rsa.Modulus);
        rsaEncryptor.ImportParameters(rsaParameters);
        var encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);

        var encryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

        var loginQueryData = new List<(string name, string value)>();

        var twoFactorCode = await GenerateSteamGuardCodeForTimeStampAsync(cancellationToken);

        if (twoFactorCode == null)
            throw new RequestException("Error generating two factor code", null, null, null);

        loginQueryData.Add(("donotcache", (await SteamTime.GetCurrentSteamTimeAsync(cancellationToken)).ToString()));
        loginQueryData.Add(("password", encryptedPassword));
        loginQueryData.Add(("username", username));
        loginQueryData.Add(("twofactorcode", twoFactorCode));
        loginQueryData.Add(("emailauth", ""));
        loginQueryData.Add(("loginfriendlyname", ""));
        loginQueryData.Add(("captchagid", "-1"));
        loginQueryData.Add(("captcha_text", ""));
        loginQueryData.Add(("emailsteamid", ""));
        loginQueryData.Add(("rsatimestamp", rsa.Timestamp));
        loginQueryData.Add(("remember_login", "true"));
        loginQueryData.Add(("oauth_client_id", "DE45CD61"));
        loginQueryData.Add(("oauth_scope", "read_profile write_profile read_client write_client"));

        var loginQuery = string.Join("&",
            loginQueryData.Select(t => $"{WebUtility.UrlEncode(t.name)}={WebUtility.UrlEncode(t.value)}"));

        var loginUrl = Endpoints.SteamCommunityUrl + "/login/dologin";

        RestResponse loginResponse;

        try
        {
            loginResponse = await RestClient.ExecutePostRequestAsync(loginUrl, cookies, null, referer,
                loginQuery, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while executing login request, exception: {exception}", e.ToJson());
            throw;
        }

        _logger.LogRestResponse(loginResponse, "LoginResponse");

        if (!loginResponse.IsSuccessful)
            throw new RequestException("Error while executing login request", loginResponse.StatusCode, null, null);

        if (loginResponse.RawBytes == null)
            throw new RequestException("Error while executing login request, raw bytes is null",
                loginResponse.StatusCode,
                null, null);

        var loginContent = await GZipDecoding.DecodeGZipAsync(loginResponse.RawBytes, _logger, cancellationToken);

        NewLoginResponse? login;

        try
        {
            login = JsonConvert.DeserializeObject<NewLoginResponse>(loginContent);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deserialize LoginResponse, content: {loginContent}, exception: {exception}",
                loginContent, e.ToJson());
            throw;
        }

        if (login == null)
            throw new RequestException("Error while executing login request, deserialize result is null",
                loginResponse.StatusCode, loginContent, null);

        _logger.LogDebug("Login message: {message}", login.Message);

        if (!login.Success)
            return login.Message;

        _logger.LogDebug("Successful login request, rewrite maFile data");

        try
        {
            var readableCookies = cookies.GetCookies(new Uri("https://steamcommunity.com"));

            var newSteamId = ulong.Parse(login.TransferParameters.SteamId);
            // var newSteamLogin = MaFile.Session.SteamId + "%7C%7C" + oAuthData.SteamLogin;
            // var newSteamLoginSecure = MaFile.Session.SteamId + "%7C%7C" + oAuthData.SteamLoginSecure;
            var newSteamLoginSecure = readableCookies["steamLoginSecure"]!.Value;
            var newSessionId = readableCookies["sessionid"]!.Value;

            var newSession = new SteamSessionData(
                newSessionId,
                newSteamLoginSecure,
                newSteamId);

            var newMaFile = new SteamMaFile(
                MaFile.SharedSecret,
                MaFile.SerialNumber,
                MaFile.RevocationCode,
                MaFile.Uri,
                MaFile.ServerTime,
                MaFile.AccountName,
                MaFile.TokenGuid,
                MaFile.IdentitySecret,
                MaFile.Secret1,
                MaFile.Status,
                MaFile.DeviceId,
                MaFile.FullyEnrolled,
                newSession);

            MaFile = newMaFile;
        }
        catch (Exception e)
        {
            _logger.LogError("Error rewrite maFile data, exception: {exception}", e.ToJson());
        }

        _logger.LogDebug("MaFile data rewrited");

        return null;
    }

    public override string ToString() => $"SteamGuardAccount, steamId: {MaFile.Session.SteamId}";

    public static SteamGuardAccount Create(string maFileContent, SteamRestClient steamRestClient,
        ISteamTime steamTime, ILogger<SteamGuardAccount> logger)
    {
        var steamMaFile = JsonConvert.DeserializeObject<SteamMaFile>(maFileContent);

        if (steamMaFile == null)
            throw new DeserializeException("Error deserialize SteamMaFile, result is null");

        return new SteamGuardAccount(steamMaFile, steamRestClient, steamTime, logger);
    }

    public static async Task<SteamGuardAccount> CreateAsync(string maFilePath, SteamRestClient steamRestClient,
        ISteamTime steamTime, ILogger<SteamGuardAccount> logger, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(maFilePath, cancellationToken);

        return Create(json, steamRestClient, steamTime, logger);
    }
}