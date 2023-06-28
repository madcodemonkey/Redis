namespace Example1;

public class CachedData<T>
{
    /// <summary>
    /// The actual cached data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// The time this item is supposed to expire in memory cache. We store it with the object
    /// because memory cache doesn't have a way to get this info. The value allows us to NOT
    /// change the memory cache expiry time when updating the <see cref="LastTimeOutOfSyncWasChecked"/> property.
    /// </summary>
    public DateTime ExpirationTime { get; set; }

    /// <summary>
    /// The last time we checked Redis to see if we were out of sync
    /// </summary>
    public DateTime LastTimeOutOfSyncWasChecked { get; set; } = DateTime.MinValue;

    /// <summary>
    /// The synchronization id that we got from Redis or one we generated if Redis has expired.
    /// </summary>
    public string SyncId { get; set; } = string.Empty;
}