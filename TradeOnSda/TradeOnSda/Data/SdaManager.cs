using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;

namespace TradeOnSda.Data;

public class SdaManager : ReactiveObservableCollection<SdaWithCredentials>
{
    private const string SettingsFileName = "settings.json";
    private const string GlobalSettingsFileName = "globalSettings.json";

    public GlobalSettings GlobalSettings { get; private set; }

    public SdaManager()
    {
        GlobalSettings = new GlobalSettings();
    }

    public void LoadFromDisk()
    {
        LoadSettings();

        LoadGlobalSettings();

        void LoadGlobalSettings()
        {
            try
            {
                if (!File.Exists(GlobalSettingsFileName))
                    return;

                var settingsContent = File.ReadAllText(GlobalSettingsFileName);

                var globalSettings = JsonConvert.DeserializeObject<GlobalSettings>(settingsContent)
                                     ?? throw new Exception();

                GlobalSettings = globalSettings;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFileName))
                    return;

                var settingsContent = File.ReadAllText(SettingsFileName);

                var savedSdaDtos = JsonConvert.DeserializeObject<SavedSdaDto[]>(settingsContent)
                                   ?? throw new Exception();

                foreach (var dto in savedSdaDtos)
                {
                    try
                    {
                        var maFileName = $"{dto.SteamId}.maFile";
                        var maFilePath = Path.Combine(Directory.GetCurrentDirectory(), "MaFiles", maFileName);

                        if (!File.Exists(maFilePath))
                            continue;

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

                        _items.Add(new SdaWithCredentials(sda, new MaFileCredentials(dto.ProxyString, dto.Password),
                            new SdaSettings(dto.AutoConfirm), this));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public async Task AddAccountAsync(SteamGuardAccount steamGuardAccount, MaFileCredentials maFileCredentials,
        SdaSettings sdaSettings)
    {
        _items.Add(new SdaWithCredentials(steamGuardAccount, maFileCredentials, sdaSettings, this));

        await SaveMaFile(steamGuardAccount);
        
        await SaveSettingsAsync();
    }

    public async Task RemoveAccountAsync(SteamGuardAccount steamGuardAccount)
    {
        var sdaWithCredentials = _items.FirstOrDefault(t => t.SteamGuardAccount == steamGuardAccount);

        if (sdaWithCredentials != null)
        {
            _items.Remove(sdaWithCredentials);

            await SaveSettingsAsync();
        }
    }

    public async Task RemoveAccountAsync(SdaWithCredentials sdaWithCredentials)
    {
        _items.Remove(sdaWithCredentials);
        await SaveSettingsAsync();
    }

    public async Task SaveSettingsAsync()
    {
        var settings = _items.Select(t =>
        {
            Debug.Assert(t != null, nameof(t) + " != null");
            return new SavedSdaDto(t.SteamGuardAccount.MaFile.Session.SteamId, t.Credentials.Password,
                t.Credentials.ProxyString, t.SdaSettings.AutoConfirm);
        });

        await File.WriteAllTextAsync(SettingsFileName, JsonConvert.SerializeObject(settings));

        var globalSettings = JsonConvert.SerializeObject(GlobalSettings);

        await File.WriteAllTextAsync(GlobalSettingsFileName, globalSettings);

        foreach (var item in _items)
        {
            await SaveMaFile(item.SteamGuardAccount);
        }
    }

    private async Task SaveMaFile(SteamGuardAccount sda)
    {
        var maFileContent = sda.MaFile.ConvertToJson();

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(),
            "MaFiles");

        Directory.CreateDirectory(directoryPath);

        var maFilePath = Path.Combine(directoryPath,
            $"{sda.MaFile.Session.SteamId}.maFile");

        await File.WriteAllTextAsync(maFilePath, maFileContent);
    }
}

public class SavedSdaDto
{
    public ulong SteamId { get; set; }

    public string Password { get; set; }

    public string? ProxyString { get; set; }

    public bool AutoConfirm { get; set; }

    public SavedSdaDto(ulong steamId, string password, string? proxyString, bool autoConfirm)
    {
        SteamId = steamId;
        Password = password;
        ProxyString = proxyString;
        AutoConfirm = autoConfirm;
    }

    [JsonConstructor]
    public SavedSdaDto()
    {
        SteamId = 0;
        Password = null!;
        ProxyString = null;
        AutoConfirm = false;
    }
}

public class SdaWithCredentials
{
    [JsonIgnore]
    private readonly SdaManager _sdaManager;
    
    public SteamGuardAccount SteamGuardAccount { get; set; }

    public MaFileCredentials Credentials { get; set; }

    public SdaSettings SdaSettings { get; set; }

    public SdaWithCredentials(SteamGuardAccount steamGuardAccount, MaFileCredentials credentials,
        SdaSettings sdaSettings, SdaManager sdaManager)
    {
        _sdaManager = sdaManager;
        SteamGuardAccount = steamGuardAccount;
        Credentials = credentials;
        SdaSettings = sdaSettings;

        Task.Run(WorkingLoop);
    }

    private async Task WorkingLoop()
    {
        var delay = TimeSpan.FromSeconds(60);

        while (true)
        {
            if (SdaSettings.AutoConfirm)
            {
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
                    await Task.Delay(delay);
                    
                    try
                    {
                        var result =
                            await SteamGuardAccount.LoginAgainAsync(SteamGuardAccount.MaFile.AccountName,
                                Credentials.Password);

                        if (result != LoginResult.LoginOkay)
                        {
                            await Task.Delay(60 * 3);
                            continue;
                        }

                        await _sdaManager.SaveSettingsAsync();
                    }
                    catch (Exception)
                    {
                        await Task.Delay(60 * 3);
                        continue;
                    }
                }
            }

            await Task.Delay(delay);
        }
     
        // ReSharper disable once FunctionNeverReturns
    }
}

public class SdaSettings
{
    public bool AutoConfirm { get; set; }

    public SdaSettings(bool autoConfirm)
    {
        AutoConfirm = autoConfirm;
    }
}