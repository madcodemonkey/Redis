using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Example1;

public class CacheStringService : ICacheStringService
{
    public const string PrefixForSync = "Sync_";
    public const string PrefixForData = "Data_";
    public const int MinimumSecondsBetweenRedisChecks = 10;
    private readonly ILogger<CacheStringService> _logger;
    private readonly IMemoryCache _memoryCache;  // Requires Microsoft.Extensions.Caching.Memory NuGt package
    private readonly IRedisService _redisService;

    /// <summary>
    /// Constructor
    /// </summary>
    public CacheStringService(ILogger<CacheStringService> logger, IMemoryCache memoryCache, IRedisService redisService)
    {
        this._logger = logger;
        this._memoryCache = memoryCache;
        this._redisService = redisService;
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

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
    public async Task<TItem?> GetAsync<TItem>(string key, TimeSpan expiry, Func<Task<TItem?>> fallback) where TItem : class
    {
        var cachedData = _memoryCache.Get<CachedData<TItem>>(key);

        // To reduce the number of hits on Redis cache, we will NOT check cache unless a certain
        // amount of time passes. This avoid us hitting cache repeatedly (sub-second) to find out
        // that nothing has changed.
        if (cachedData != null)
        {
            var lastCheck = DateTime.UtcNow - cachedData.LastTimeOutOfSyncWasChecked;
            if (lastCheck.TotalSeconds < MinimumSecondsBetweenRedisChecks)
            {
                return cachedData.Data;
            }
        }

        var redisSyncId = await _redisService.StringGetAsync($"{PrefixForSync}{key}");
      
        // If Redis has data, compare it to what we have locally
        if (string.IsNullOrWhiteSpace(redisSyncId) == false)
        {
            // If we don't have anything locally or we don't match what is in Redis, pull data from
            // Redis cache.
            if (cachedData == null || cachedData.SyncId != redisSyncId)
            {
                string? dataAsString = await _redisService.StringGetAsync($"{PrefixForData}{key}");
                if (string.IsNullOrWhiteSpace(dataAsString) == false)
                {
                    try
                    {
                        var someObject = JsonConvert.DeserializeObject<TItem>(dataAsString,
                            settings: new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                        cachedData = new CachedData<TItem>
                        {
                            Data = someObject,
                            ExpirationTime = DateTime.UtcNow + TimeSpan.FromMinutes(expiry.TotalMinutes * 2),
                            LastTimeOutOfSyncWasChecked = DateTime.UtcNow,
                            SyncId = redisSyncId
                        };

                        UpdateMemoryCache(key, cachedData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Unable to deserialize this string found in Redis cache: {dataAsString}");
                        await _redisService.ClearKeyAsync(key);
                    }
                }
            }
            else if (cachedData.SyncId == redisSyncId)
            {
                // Cache matches!! Update our last check time.
                cachedData.LastTimeOutOfSyncWasChecked = DateTime.UtcNow;
                UpdateMemoryCache(key, cachedData);
            }
        }

        // If Redis didn't have any data for this key (because it has expired or was empty) OR we
        // had a deserialization error above OR for some other reason memory cache is still blank,
        // we'll need to refresh data using the fallback function.
        if (string.IsNullOrWhiteSpace(redisSyncId) || cachedData == null)
        {
            cachedData = await GetDataFromFallbackAsync(expiry, fallback);

            // Update Redis cache with new fresh data if the fallback could find some.
            if (cachedData != null)
            {
                UpdateMemoryCache(key, cachedData);

                // WARNING 1: The JsonSerializerSettings used below will cause us to prune off parts
                // of an object. For example, on the SystemApplication object, it will prune off
                // SystemApplicationPermissions collection because those elements refer back to
                // SystemApplication in one of its fields (the FK field pointing back to its
                // parent). The takeaway here is that you cannot serialize everything and it's
                // especially problematic with objects obtained from the EF context. Failure to use
                // the settings can result in the following error: "Self referencing loop detected
                // for property ....".

                // WARNING 2: If you aren't seeing the behavior described in warning 1, let the app
                // serialize and store the data in Redis and then stop the application and rerun it
                // again. The EF Context, if it has certain data in memory, can "fill in the blanks"
                // for you automatically. By restarting, you flush it out the EF context and then
                // you'll see the problems with the missing collections. In the case with the
                // SystemApplication object, the roles collection is blank when I re-run the second
                // time and I get 403 Forbidden errors back in Postman.
                string serializedData = JsonConvert.SerializeObject(cachedData.Data, settings: new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                await _redisService.StringSetAsync($"{PrefixForSync}{key}", cachedData.SyncId, expiry);
                await _redisService.StringSetAsync($"{PrefixForData}{key}", serializedData, expiry);
            }
        }

        // If the fallback cannot find data and it was not in Redis, clear memory cache and return
        // some defaults.
        if (cachedData == null)
        {
            _memoryCache.Remove(key);
            return default(TItem);
        }

        return cachedData.Data;
    }

    /// <summary>
    /// Bust the cache by using the fallback location (most likely the database) and update memory
    /// cache and Redis
    /// </summary>
    /// <typeparam name="TItem">The type of data</typeparam>
    /// <param name="expiry">The time you would like the key to expire</param>
    /// <param name="fallback">
    /// The fallback method to call (usually the database) to get refreshed data.
    /// </param>
    /// <returns>A Cache item or null</returns>
    private async Task<CachedData<TItem>?> GetDataFromFallbackAsync<TItem>(TimeSpan expiry, Func<Task<TItem?>> fallback) where TItem : class
    {
        // Fallback is the database in most cases.
        var fallbackData = await fallback();
        if (fallbackData == null)
        {
            return null;
        }

        // Update memory cache
        var cachedData = new CachedData<TItem>
        {
            Data = fallbackData,
            ExpirationTime = DateTime.UtcNow + TimeSpan.FromMinutes(expiry.TotalMinutes * 2),
            LastTimeOutOfSyncWasChecked = DateTime.UtcNow,
            SyncId = Guid.NewGuid().ToString()
        };

        return cachedData;
    }

   
    /// <summary>
    /// Updates memory cache. Memory cache holds onto data twice as long as Redis cache to avoid a
    /// race condition where clocks are slightly out of sync and we retrieve new data locally due to
    /// memory cache expiring and are pushing the Redis expiry as opposed to Redis being the reason
    /// why things update.
    /// </summary>
    /// <param name="key">The key to update</param>
    /// <param name="cachedData">The data to store in the cache</param>
    private void UpdateMemoryCache<T>(string key, CachedData<T>? cachedData)
    {
        if (cachedData == null)
        {
            return;
        }

        var expiry = cachedData.ExpirationTime - DateTime.UtcNow;
        _memoryCache.Set(key, cachedData, TimeSpan.FromMinutes(expiry.TotalMinutes * 2));
    }
}