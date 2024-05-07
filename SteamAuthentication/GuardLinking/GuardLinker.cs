using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SteamAuthentication.Exceptions;
using SteamAuthentication.Logic;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;

namespace SteamAuthentication.GuardLinking;

public class GuardLinker
{
    private readonly IPhoneNumberProvider _phoneNumberProvider;
    private readonly IWebProxy? _proxy;
    private readonly ISteamTime _steamTime;
    private readonly string _username;
    private readonly string _password;
    private readonly IAuthenticator _authenticator;
    private readonly string _deviceId;
    private readonly NoMaFileRestClient _noMaFileRestClient;

    public GuardLinker(IPhoneNumberProvider phoneNumberProvider, IWebProxy? proxy, ISteamTime steamTime,
        string username, string password,
        IAuthenticator authenticator)
    {
        _phoneNumberProvider = phoneNumberProvider;
        _proxy = proxy;
        _steamTime = steamTime;
        _username = username;
        _password = password;
        _authenticator = authenticator;
        _deviceId = GenerateDeviceId();
        _noMaFileRestClient = new NoMaFileRestClient(proxy);
    }

    public async Task<(CredentialsAuthSession authSession, AuthPollResult pollResponse, ulong steamId)>
        StartLinkingGuardAsync(CancellationToken cancellationToken = default)
    {
        var configuration = SteamConfiguration.Create(builder => builder.WithHttpClientFactory(
            () =>
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = _proxy,
                };

                var client = new HttpClient(httpClientHandler);

                return client;
            })
            .WithProtocolTypes(ProtocolTypes.WebSocket));

        var steamClient = new SteamClient(configuration);
        steamClient.ConnectWithProxy(null, _proxy);

        while (!steamClient.IsConnected)
            await Task.Delay(500, cancellationToken);

        var authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
        {
            Username = _username,
            Password = _password,
            IsPersistentSession = false,
            PlatformType = EAuthTokenPlatformType.k_EAuthTokenPlatformType_MobileApp,
            ClientOSType = EOSType.AndroidUnknown,
            Authenticator = _authenticator,
            WebsiteID = "Mobile",
        });

        var pollResponse = await authSession.PollingWaitForResultAsync(cancellationToken);

        var steamId = authSession.SteamID.ConvertToUInt64();

        return (authSession, pollResponse, steamId);
    }

    public async Task FinalizeAddGuardAsync(string smsCode, SteamMaFile maFile,
        AuthPollResult pollResult,
        CancellationToken cancellationToken = default)
    {
        var timestamp = await _steamTime.GetCurrentSteamTimeAsync(cancellationToken);

        var guardCode =
            SteamGuardCodeGenerating.GenerateSteamGuardCode(maFile.SharedSecret, timestamp, NullLogger.Instance);

        var query = new Dictionary<string, string>
        {
            { "steamid", maFile.Session!.SteamId.ToString() },
            { "authenticator_code", guardCode },
            { "authenticator_time", (await _steamTime.GetCurrentSteamTimeAsync(cancellationToken)).ToString() },
            { "activation_code", smsCode },
            { "validate_sms_code", "1" },
        };

        var url =
            $"https://api.steampowered.com/ITwoFactorService/FinalizeAddAuthenticator/v1/?access_token={pollResult.AccessToken}";

        var response = await _noMaFileRestClient.SendPostAsync(url, query.ToQuery(), cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing FinalizeAddAuthenticator request", response.StatusCode,
                null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing FinalizeAddAuthenticator request, raw bytes is null",
                response.StatusCode,
                null, null);

        var finalizeContent =
            await GZipDecoding.DecodeGZipAsync(response.RawBytes, NullLogger.Instance, cancellationToken);

        FinalizeAuthenticatorResponseWrapper? finalizeResponse;

        try
        {
            finalizeResponse = JsonConvert.DeserializeObject<FinalizeAuthenticatorResponseWrapper>(finalizeContent);
        }
        catch (Exception e)
        {
            throw new RequestException("Error while deserializing FinalizeAddAuthenticator response",
                response.StatusCode,
                finalizeContent, e);
        }

        if (finalizeResponse?.Response == null)
            throw new RequestException("Error while deserializing FinalizeAddAuthenticator response, result is null",
                response.StatusCode,
                finalizeContent, null);

        if (finalizeResponse.Response.Status == 89)
            throw new RequestException("Wrong SMS code", response.StatusCode, response.Content, null);

        if (!finalizeResponse.Response.Success)
            throw new RequestException("Error while FinalizeAddAuthenticator", response.StatusCode, response.Content,
                null);
    }

    public async Task<SteamMaFile> SendAddGuardRequestAsync(ulong steamId,
        AuthPollResult pollResult, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>
        {
            { "steamid", steamId.ToString() },
            { "authenticator_time", (await _steamTime.GetCurrentSteamTimeAsync(cancellationToken)).ToString() },
            { "authenticator_type", "1" },
            { "device_identifier", _deviceId },
            { "sms_phone_id", "1" }
        };

        var url =
            $"https://api.steampowered.com/ITwoFactorService/AddAuthenticator/v1/?access_token={pollResult.AccessToken}";

        var response = await _noMaFileRestClient.SendPostAsync(url, query.ToQuery(), cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing AddAuthenticator request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing AddAuthenticator request, raw bytes is null",
                response.StatusCode,
                null, null);

        var addGuardContent =
            await GZipDecoding.DecodeGZipAsync(response.RawBytes, NullLogger.Instance, cancellationToken);

        AddGuardResponseWrapper? addGuardResponse;

        try
        {
            addGuardResponse = JsonConvert.DeserializeObject<AddGuardResponseWrapper>(addGuardContent);
        }
        catch (Exception e)
        {
            throw new RequestException("Error while deserializing AddAuthenticator response", response.StatusCode,
                addGuardContent, e);
        }

        if (addGuardResponse?.Response == null)
            throw new RequestException("Error while deserializing AddAuthenticator response, result is null",
                response.StatusCode,
                addGuardContent, null);

        if (addGuardResponse.Response.Status == 2)
        {
            var phoneNumber = await _phoneNumberProvider.GetPhoneNumberAsync(cancellationToken);

            var userCountry = await GetUserCountryAsync(steamId, pollResult.AccessToken, cancellationToken);

            var email = await SetPhoneNumberAsync(phoneNumber, userCountry, pollResult.AccessToken, cancellationToken);

            throw new PhoneNumberException(email);
        }

        if (addGuardResponse.Response.Status != 1)
            throw new RequestException($"Error add authenticator, status is {addGuardResponse.Response.Status}",
                response.StatusCode, addGuardContent, null);

        var steamMaFile = new SteamMaFile(
            addGuardResponse.Response.SharedSecret,
            addGuardResponse.Response.SerialNumber,
            addGuardResponse.Response.RevocationCode,
            addGuardResponse.Response.Uri,
            addGuardResponse.Response.ServerTime,
            addGuardResponse.Response.AccountName,
            addGuardResponse.Response.TokenGid,
            addGuardResponse.Response.IdentitySecret,
            addGuardResponse.Response.Secret1,
            addGuardResponse.Response.Status,
            _deviceId,
            true,
            new SteamSessionData("", "", steamId));

        return steamMaFile;
    }

    private async Task<string> GetUserCountryAsync(ulong steamId, string accessToken,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string>();
        query.Add("steamid", steamId.ToString());

        var url = $"https://api.steampowered.com/IUserAccountService/GetUserCountry/v1?access_token={accessToken}";

        var response = await _noMaFileRestClient.SendPostAsync(url, query.ToQuery(), cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing GetUserCountry request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing GetUserCountry request, raw bytes is null",
                response.StatusCode,
                null, null);

        var getUserCountryContent =
            await GZipDecoding.DecodeGZipAsync(response.RawBytes, NullLogger.Instance, cancellationToken);

        GetUserCountryResponseWrapper? addGuardResponse;

        try
        {
            addGuardResponse = JsonConvert.DeserializeObject<GetUserCountryResponseWrapper>(getUserCountryContent);
        }
        catch (Exception e)
        {
            throw new RequestException("Error while deserializing GetUserCountry response", response.StatusCode,
                getUserCountryContent, e);
        }

        if (addGuardResponse?.Response == null)
            throw new RequestException("Error while deserializing GetUserCountry response, result is null",
                response.StatusCode,
                getUserCountryContent, null);

        if (addGuardResponse.Response.Country == null)
            throw new RequestException("Error get user country", null, null, null);

        return addGuardResponse.Response.Country;
    }

    public async Task ConfirmPhoneNumberAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var url = $"https://api.steampowered.com/IPhoneService/SendPhoneVerificationCode/v1?access_token={accessToken}";

        var response = await _noMaFileRestClient.SendPostAsync(url, "", cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing SendPhoneVerificationCode request", response.StatusCode,
                null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing SendPhoneVerificationCode request, raw bytes is null",
                response.StatusCode,
                null, null);
    }

    private async Task<string?> SetPhoneNumberAsync(string phoneNumber, string userCountry, string accessToken,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string>();

        query.Add("phone_number", phoneNumber);
        query.Add("phone_country_code", userCountry);

        var url = $"https://api.steampowered.com/IPhoneService/SetAccountPhoneNumber/v1?access_token={accessToken}";

        var response = await _noMaFileRestClient.SendPostAsync(url, query.ToQuery(), cancellationToken);

        if (!response.IsSuccessful)
            throw new RequestException("Error while executing SetAccountPhoneNumber request", response.StatusCode, null,
                null);

        if (response.RawBytes == null)
            throw new RequestException("Error while executing SetAccountPhoneNumber request, raw bytes is null",
                response.StatusCode,
                null, null);

        var setPhoneNumberContent =
            await GZipDecoding.DecodeGZipAsync(response.RawBytes, NullLogger.Instance, cancellationToken);

        SetPhoneNumberResponseWrapper? setPhoneNumberResponse;

        try
        {
            setPhoneNumberResponse =
                JsonConvert.DeserializeObject<SetPhoneNumberResponseWrapper>(setPhoneNumberContent);
        }
        catch (Exception e)
        {
            throw new RequestException("Error while deserializing SetAccountPhoneNumber response", response.StatusCode,
                setPhoneNumberContent, e);
        }

        if (setPhoneNumberResponse?.Response == null)
            throw new RequestException("Error while deserializing SetAccountPhoneNumber response, result is null",
                response.StatusCode,
                setPhoneNumberContent, null);

        return setPhoneNumberResponse.Response.ConfirmationEmailAddress;
    }

    private static string GenerateDeviceId() => "android:" + Guid.NewGuid();
}