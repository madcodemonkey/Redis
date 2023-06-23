using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Example1;

public class RedisService : IRedisService, IDisposable
{
    private readonly ILogger<RedisService> _logger;
    private readonly ExampleSettings _settings;
    private ConnectionMultiplexer? _redis;

    /// <summary>
    /// Constructor
    /// </summary>
    public RedisService(ILogger<RedisService> logger, ExampleSettings settings)
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
    /// Returns all fields and values of the hash stored at key.
    /// <seealso href="https://redis.io/commands/hgetall"/>
    /// </summary>
    /// <param name="key">The key of the hash to get all entries from.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>List of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
    public async Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        try
        {
            var db = GetDatabase();
            return await db.HashGetAllAsync(key, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to get ALL hash entries for the key equal '{key}'");
            return Array.Empty<HashEntry>();
        }
    }

    /// <summary>
    /// Returns the value associated with field in the hash stored at key.
    /// </summary>
    /// <param name="key">The key of the hash.</param>
    /// <param name="hashField">The field in the hash to get.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>The value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
    /// <remarks><seealso href="https://redis.io/commands/hget"/></remarks>
    public async Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        try
        {
            var db = GetDatabase();
            return await db.HashGetAsync(key, hashField, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to get a hash entry with a key equal '{key}'");
            return default(RedisValue);
        }
    }

    /// <summary>
    /// Sets the specified fields to their respective values in the hash stored at key.
    /// This command overwrites any specified fields that already exist in the hash, leaving other unspecified fields untouched.
    /// If key does not exist, a new key holding a hash is created.
    /// </summary>
    /// <param name="key">The key of the hash.</param>
    /// <param name="hashFields">The entries to set in the hash.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <remarks><seealso href="https://redis.io/commands/hmset"/></remarks>
    public async Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        try
        {
            var db = GetDatabase();
            await db.HashSetAsync(key, hashFields, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to set a hash entry with a key equal '{key}'");
        }
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
        TimeSpan? expiry = null, bool keepTtl = false,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        try
        {
            var db = GetDatabase();
            return await db.StringSetAsync(key, value, expiry, keepTtl, when, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to set a string with a key equal '{key}'");
            return false;
        }
    }

    /// <summary>
    /// Execute an arbitrary command against the server; this is primarily intended for executing modules,
    /// but may also be used to provide access to new features that lack a direct API.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="args">The arguments to pass for the command.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>A dynamic representation of the command's result.</returns>
    /// <remarks>This API should be considered an advanced feature; inappropriate use can be harmful.</remarks>
    public async Task<RedisResult> ExecuteAsync(string command, CommandFlags flags = CommandFlags.None, params object[] args)
    {
        try
        {
            var db = GetDatabase();
            return await db.ExecuteAsync(command, args, flags);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to execute command '{command}'");
            return default(RedisResult);
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