namespace Example1;

public interface ICacheHashSetService : IDisposable
{
    /// <summary>
    /// Gets data from cache
    /// </summary>
    /// <typeparam name="TItem">The type of the item you stored in cache</typeparam>
    /// <param name="key">The key to find</param>
    /// <param name="expiry">The time you would like the key to expire</param>
    /// <param name="fallback">
    /// A fallback function that can retrieve the data if it is not in cache. It usually calls a database.
    /// </param>
    /// <returns>The item you or null</returns>
    Task<TItem?> GetAsync<TItem>(string key, TimeSpan expiry, Func<Task<TItem?>> fallback) where TItem : class;
}