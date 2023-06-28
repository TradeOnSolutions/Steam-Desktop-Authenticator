using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using TradeOnSda.Exceptions;

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
            var confirmations = (await SteamGuardAccount.FetchConfirmationAsync())
                .Where(t => t.ConfirmationType is ConfirmationType.MarketSellTransaction
                    or ConfirmationType.Trade);

            foreach (var confirmation in confirmations)
            {
                await SteamGuardAccount.AcceptConfirmationAsync(confirmation);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception)
        {
            await Task.Delay(SdaSettings.AutoConfirmDelay);

            try
            {
                var result =
                    await SteamGuardAccount.LoginAgainAsync(SteamGuardAccount.MaFile.AccountName,
                        Credentials.Password);

                if (result != LoginResult.LoginOkay)
                {
                    await Task.Delay(SdaSettings.AutoConfirmDelay * 5);
                    return;
                }

                await _sdaManager.SaveMaFile(SteamGuardAccount);
            }
            catch (Exception)
            {
                await Task.Delay(SdaSettings.AutoConfirmDelay * 5);
            }
        }
    }

    public static SdaWithCredentials FromDto(SavedSdaDto dto, SdaManager sdaManager)
    {
        var maFileName = $"{dto.SteamId}.maFile";
        var maFilePath = Path.Combine(Directory.GetCurrentDirectory(), "MaFiles", maFileName);

        if (!File.Exists(maFilePath))
            throw new LoadMaFileException("MaFile not found");

        var proxy = ProxyLogic.ParseWebProxy(dto.ProxyString);

        var maFileContent = File.ReadAllText(maFilePath);

        var maFile = JsonConvert.DeserializeObject<SteamMaFile>(maFileContent) ??
                     throw new Exception("SteamMaFile is null");

        var steamTime = new SteamTime();

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