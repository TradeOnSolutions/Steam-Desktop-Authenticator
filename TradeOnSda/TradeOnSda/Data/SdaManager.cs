using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuthentication.LogicModels;

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
                        var sdaWithCredentials = SdaWithCredentials.FromDto(dto, this);
                        
                        _items.Add(sdaWithCredentials);
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
        
        await SaveEverythingAsync();
    }

    public async Task RemoveAccountAsync(SdaWithCredentials sdaWithCredentials)
    {
        _items.Remove(sdaWithCredentials);
        
        await SaveEverythingAsync();
    }

    public async Task SaveEverythingAsync()
    {
        await SaveSettingsAsync();
        
        await SaveGlobalSettingsAsync();
        
        await SaveAllMaFilesAsync();
    }

    public async Task SaveSettingsAsync()
    {
        var settings = _items.Select(t =>
        {
            Debug.Assert(t != null, nameof(t) + " != null");
            return t.ToDto();
        });

        await File.WriteAllTextAsync(SettingsFileName, JsonConvert.SerializeObject(settings));
    }

    public async Task SaveGlobalSettingsAsync()
    {
        var globalSettings = JsonConvert.SerializeObject(GlobalSettings);

        await File.WriteAllTextAsync(GlobalSettingsFileName, globalSettings);
    }

    public async Task SaveAllMaFilesAsync()
    {
        foreach (var item in _items) 
            await SaveMaFile(item.SteamGuardAccount);
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