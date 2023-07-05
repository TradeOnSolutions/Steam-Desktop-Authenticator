namespace SteamAuthentication.GuardLinking;

public interface IPhoneNumberProvider
{
    public Task<string> GetPhoneNumberAsync(CancellationToken cancellationToken);
}