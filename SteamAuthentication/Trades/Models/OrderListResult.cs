namespace SteamAuthentication.Trades.Models;

public class OrderListResult<T>
    where T : class
{
    public int TotalCount { get; }
    
    public IEnumerable<T> Orders { get; }

    public OrderListResult(int totalCount, IEnumerable<T> orders)
    {
        TotalCount = totalCount;
        Orders = orders;
    }
}