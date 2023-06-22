namespace SteamAuthentication.Logic;

internal static class TryHelpers
{
    public static async Task<T?> TryAsync<T>(Task<T> task)
        where T : class
    {
        try
        {
            var result = await task;

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<bool> TryAsync(Task task)
    {
        try
        {
            await task;

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }
}