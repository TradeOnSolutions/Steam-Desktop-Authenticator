using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SteamAuthentication.Exceptions;
using SteamAuthentication.Logic;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;

namespace TradeOnSda.Data;

public class SdaWithCredentials
{
    private readonly SdaManager _sdaManager;

    public SteamGuardAccount SteamGuardAccount { get; set; }

    public MaFileCredentials Credentials { get; set; }

    public SdaSettings SdaSettings { get; set; }
    
    public SdaState SdaState { get; }

    public SdaWithCredentials(SteamGuardAccount steamGuardAccount, MaFileCredentials credentials,
        SdaSettings sdaSettings, SdaManager sdaManager)
    {
        _sdaManager = sdaManager;
        SteamGuardAccount = steamGuardAccount;
        Credentials = credentials;
        SdaSettings = sdaSettings;

        SdaState = new SdaState();

        Task.Run(AutoConfirmWorkingLoop);
    }

    private async Task AutoConfirmWorkingLoop()
    {
        while (true)
        {
            await ProcessAutoConfirmAsync();
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private async Task ProcessAutoConfirmAsync()
    {
        if (!SdaSettings.IsEnabledAutoConfirm)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            return;
        }

        try
        {
            if (SteamGuardAccount.MaFile?.Session is SteamSessionData sessionData)
            {
                var accessToken = sessionData.SteamLoginSecure?.Split("%7C%7C")[1];
                var refreshToken = sessionData.RefreshToken;

                if (refreshToken is not null &&
                    accessToken is not null &&
                    JwtTokenValidator.IsTokenExpired(accessToken))
                {
                    var steamId = sessionData.SteamId;

                    var result =
                        await SteamGuardAccount.TryGenerateAccessTokenAsync(steamId, refreshToken);

                    if (result is null)
                    {
                        await _sdaManager.SaveMaFile(SteamGuardAccount);
                    }
                }
            }

            var confirmations = (await SteamGuardAccount.FetchConfirmationAsync())
                .Where(t => t.ConfirmationType is ConfirmationType.MarketSellTransaction
                    or ConfirmationType.Trade).ToArray();

            if (confirmations.Length == 0)
            {
                await Task.Delay(SdaSettings.AutoConfirmDelay);
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            await SteamGuardAccount.AcceptConfirmationsAsync(confirmations.ToArray());

            await Task.Delay(SdaSettings.AutoConfirmDelay);
        }
        catch (RequestException e) when (e.HttpStatusCode == HttpStatusCode.Unauthorized)
        {
            await Task.Delay(SdaSettings.AutoConfirmDelay);

            var result =
                await SteamGuardAccount.TryLoginAgainAsync(SteamGuardAccount.MaFile.AccountName,
                    Credentials.Password);

            if (result is null)
            {
                await _sdaManager.SaveMaFile(SteamGuardAccount);
                return;
            }

            await Task.Delay(SdaSettings.AutoConfirmDelay * 5);
        }
        catch (RequestException e) when (e.HttpStatusCode == HttpStatusCode.TooManyRequests)
        {
            await Task.Delay(3 * SdaSettings.AutoConfirmDelay);
        }
        catch (Exception)
        {
            await Task.Delay(SdaSettings.AutoConfirmDelay);
        }
    }

    public static async Task<SdaWithCredentials> FromDto(SavedSdaDto dto, SdaManager sdaManager)
    {
        var maFileName = $"MaFiles/{dto.SteamId}.maFile";
        
        if (!sdaManager.FileSystemAdapterProvider.GetAdapter().ExistsFile(maFileName))
            throw new Exception("MaFile not found");

        var maFileContent = await sdaManager.FileSystemAdapterProvider.GetAdapter()
            .ReadFileAsync(maFileName, CancellationToken.None);

        var maFile = JsonConvert.DeserializeObject<SteamMaFile>(maFileContent) ??
                     throw new Exception("SteamMaFile is null");
        
        var proxy = ProxyLogic.ParseWebProxy(dto.ProxyString);

        var steamTime = sdaManager.GlobalSteamTime;

        var sda = new SteamGuardAccount(
            maFile,
            new SteamRestClient(proxy),
            steamTime,
            NullLogger<SteamGuardAccount>.Instance);

        return new SdaWithCredentials(sda, new MaFileCredentials(
                proxy,
                dto.ProxyString,
                dto.Password),
            new SdaSettings(dto.AutoConfirm, dto.AutoConfirmDelay), sdaManager);
    }

    public SavedSdaDto ToDto()
    {
        return new SavedSdaDto(SteamGuardAccount.MaFile.Session.SteamId,
            Credentials.Password,
            Credentials.ProxyString,
            SdaSettings.IsEnabledAutoConfirm,
            SdaSettings.AutoConfirmDelay);
    }
}