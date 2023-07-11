using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuthentication.LogicModels;
using TradeOnSda.Data.FileSystemAdapters;

namespace TradeOnSda.Data;

public class SdaManager : ReactiveObservableCollection<SdaWithCredentials>
{
    private const string SettingsFileName = "settings.json";
    private const string GlobalSettingsFileName = "globalSettings.json";

    public GlobalSettings GlobalSettings { get; private set; }

    private readonly FileSystemAdapterProvider _fileSystemAdapterProvider;

    public static async Task<SdaManager> CreateSdaManagerAsync()
    {
        var sdaManager = new SdaManager();

        await sdaManager.LoadFromDiskAsync();

        return sdaManager;
    }

    private SdaManager()
    {
        GlobalSettings = new GlobalSettings();

        _fileSystemAdapterProvider = new FileSystemAdapterProvider();

        Task.Run(CheckProxiesWorkingLoop);
    }

    private async Task CheckProxiesWorkingLoop()
    {
        while (true)
        {
            try
            {
                var sdas = Items
                    .Where(t => t.Credentials.Proxy != null)
                    .ToArray();

                var withoutProxySdas = Items
                    .Where(t => t.Credentials.Proxy == null);

                foreach (var item in withoutProxySdas)
                    item.SdaState.ProxyState = ProxyState.Unknown;

                foreach (var sda in sdas)
                {
                    try
                    {
                        Debug.Assert(sda.Credentials.Proxy != null, "sda.Credentials.Proxy != null");

                        var result = await ProxyChecking.CheckProxyAsync(sda.Credentials.Proxy);

                        sda.SdaState.ProxyState = result ? ProxyState.Ok : ProxyState.Error;
                    }
                    catch (Exception)
                    {
                        sda.SdaState.ProxyState = ProxyState.Error;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private async Task LoadFromDiskAsync()
    {
        await LoadSettingsAsync();

        await LoadGlobalSettingsAsync();

        async Task LoadGlobalSettingsAsync()
        {
            try
            {
                if (!_fileSystemAdapterProvider.GetAdapter().ExistsFile(GlobalSettingsFileName))
                    return;

                var settingsContent = await _fileSystemAdapterProvider.GetAdapter()
                    .ReadFileAsync(GlobalSettingsFileName, CancellationToken.None);

                var globalSettings = JsonConvert.DeserializeObject<GlobalSettings>(settingsContent)
                                     ?? throw new Exception();

                GlobalSettings = globalSettings;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        async Task LoadSettingsAsync()
        {
            try
            {
                if (!_fileSystemAdapterProvider.GetAdapter().ExistsFile(SettingsFileName))
                    return;

                var settingsContent = await _fileSystemAdapterProvider.GetAdapter()
                    .ReadFileAsync(SettingsFileName, CancellationToken.None);

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

        await SaveSettingsAsync();
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
            return t.ToDto();
        });

        await _fileSystemAdapterProvider.GetAdapter().WriteFileAsync(SettingsFileName,
            JsonConvert.SerializeObject(settings), CancellationToken.None);
    }

    public async Task SaveGlobalSettingsAsync()
    {
        var globalSettings = JsonConvert.SerializeObject(GlobalSettings);

        await _fileSystemAdapterProvider.GetAdapter()
            .WriteFileAsync(GlobalSettingsFileName, globalSettings, CancellationToken.None);
    }

    public async Task SaveMaFile(SteamGuardAccount sda)
    {
        var maFileContent = sda.MaFile.ConvertToJson();

        var maFilePath = Path.Combine("MaFiles",
            $"{sda.MaFile.Session?.SteamId}.maFile");

        await _fileSystemAdapterProvider.GetAdapter().WriteFileAsync(maFilePath, maFileContent, CancellationToken.None);
    }
}