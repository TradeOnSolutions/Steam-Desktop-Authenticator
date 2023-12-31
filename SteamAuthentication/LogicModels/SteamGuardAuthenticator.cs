using SteamKit2.Authentication;

namespace SteamAuthentication.LogicModels;

public class SteamGuardAuthenticator : IAuthenticator
{
    private readonly SteamGuardAccount _steamGuardAccount;

    public SteamGuardAuthenticator(SteamGuardAccount steamGuardAccount)
    {
        _steamGuardAccount = steamGuardAccount;
    }

    public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) =>
        await _steamGuardAccount.GenerateSteamGuardCodeForTimeStampAsync();

    public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect) => Task.FromResult(string.Empty);

    public Task<bool> AcceptDeviceConfirmationAsync() => Task.FromResult(false);
}