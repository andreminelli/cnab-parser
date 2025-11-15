namespace CnabParser.Application.Tests;

public static class AsyncEnumerableExtensions
{
    //  When upgraded to .Net 9+, this method could be removed.
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var results = new List<T>();

        await foreach (var value in asyncEnumerable)
        {
            results.Add(value);
        }

        return results;
    }
}
