using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;

namespace TradeOnSda.Data;

public class SdaManager : ReactiveObservableCollection<SdaWithCredentials>
{
    private const string SettingsFileName = "settings.json";

    public void LoadFromDisk()
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
                    var maFileName = $"{dto.SteamId}.json";
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
                        new SteamRestClient(new HttpClient(), proxy),
                        steamTime,
                        NullLogger<SteamGuardAccount>.Instance);

                    _items.Add(new SdaWithCredentials(sda, new MaFileCredentials(dto.ProxyString, dto.Password)));
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

    public async Task AddAccountAsync(SteamGuardAccount steamGuardAccount, MaFileCredentials maFileCredentials)
    {
        _items.Add(new SdaWithCredentials(steamGuardAccount, maFileCredentials));

        await SaveSettingsAsync();

        var maFileContent = steamGuardAccount.MaFile.ConvertToJson();

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(),
            "MaFiles");

        Directory.CreateDirectory(directoryPath);

        var maFilePath = Path.Combine(directoryPath,
            $"{steamGuardAccount.MaFile.Session.SteamId}.json");

        await File.WriteAllTextAsync(maFilePath, maFileContent);
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
                t.Credentials.ProxyString);
        });

        await File.WriteAllTextAsync(SettingsFileName, JsonConvert.SerializeObject(settings));
    }
}

public class SavedSdaDto
{
    public ulong SteamId { get; set; }

    public string Password { get; set; }

    public string? ProxyString { get; set; }

    public SavedSdaDto(ulong steamId, string password, string? proxyString)
    {
        SteamId = steamId;
        Password = password;
        ProxyString = proxyString;
    }
}

public class SdaWithCredentials
{
    public SteamGuardAccount SteamGuardAccount { get; set; }

    public MaFileCredentials Credentials { get; set; }

    public SdaWithCredentials(SteamGuardAccount steamGuardAccount, MaFileCredentials credentials)
    {
        SteamGuardAccount = steamGuardAccount;
        Credentials = credentials;
    }
}