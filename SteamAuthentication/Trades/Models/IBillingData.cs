namespace SteamAuthentication.Trades.Models;

public interface IBillingData
{
    public string FirstName { get; }

    public string LastName { get; }

    public string BillingAddress { get; }

    public string BillingAddress2 { get; }

    public string City { get; }

    public string State { get; }

    public string Zip { get; }

    public string Country { get; }
}