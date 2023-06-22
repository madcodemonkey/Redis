using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Example1;

public interface IRedisService
{
    IDatabase GetDatabase();

    /// <summary>
    /// Get the value of key. If the key does not exist the special value nil is returned.
    /// An error is returned if the value stored at key is not a string, because GET only handles string values.
    /// <seealso href="https://redis.io/commands/get" />
    /// </summary>
    /// <param name="key">The key of the string.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>The value of key, or nil when key does not exist.</returns>
    Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

    /// <summary>
    /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
    /// </summary>
    /// <param name="key">The key of the string.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="expiry">The expiry to set.</param>
    /// <param name="keepTtl">Whether to maintain the existing key's TTL (KEEPTTL flag).</param>
    /// <param name="when">Which condition to set the value under (defaults to always).</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns><see langword="true" /> if the string was set, <see langword="false" /> otherwise.</returns>
    /// <remarks><seealso href="https://redis.io/commands/set" /></remarks>
    Task<bool> StringSetAsync(RedisKey key, RedisValue value,
        TimeSpan? expiry = null, bool keepTtl = false,
        When when = When.Always, CommandFlags flags = CommandFlags.None);


}

public class RedisService : IRedisService, IDisposable
{
    private readonly ILogger<RedisService> _logger;
    private readonly ExampleSettings _settings;
    private ConnectionMultiplexer? _redis;

    /// <summary>
    /// Constructor
    /// </summary>
    public RedisService(ILogger<RedisService> logger,  ExampleSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public IDatabase GetDatabase()
    {
        _redis ??= ConnectionMultiplexer.Connect(_settings.RedisConnection);

        return _redis.GetDatabase();
    }

    /// <summary>
    /// Get the value of key. If the key does not exist the special value nil is returned.
    /// An error is returned if the value stored at key is not a string, because GET only handles string values.
    /// <seealso href="https://redis.io/commands/get" />
    /// </summary>
    /// <param name="key">The key of the string.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>The value of key, or nil when key does not exist.</returns>
    public async Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        try
        {
            var db = GetDatabase();
            return await db.StringGetAsync(key, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to retrieve a string with a key equal '{key}'");
             return default(RedisValue);
        }
    }

    /// <summary>
    /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
    /// </summary>
    /// <param name="key">The key of the string.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="expiry">The expiry to set.</param>
    /// <param name="keepTtl">Whether to maintain the existing key's TTL (KEEPTTL flag).</param>
    /// <param name="when">Which condition to set the value under (defaults to always).</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns><see langword="true" /> if the string was set, <see langword="false" /> otherwise.</returns>
    /// <remarks><seealso href="https://redis.io/commands/set" /></remarks>
    public async Task<bool> StringSetAsync(RedisKey key, RedisValue value,
        TimeSpan? expiry = null,  bool keepTtl = false,
        When when = When.Always,  CommandFlags flags = CommandFlags.None)
    {
        try
        {
            var db = GetDatabase();
            return await db.StringSetAsync(key, value, expiry ,keepTtl, when, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to set a string with a key equal '{key}'");
            return false;
        }
    }

    public void Dispose()
    {
        if (_redis != null)
        {
            _redis.Dispose();
            _redis = null;
        }
    }
}