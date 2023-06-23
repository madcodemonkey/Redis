using StackExchange.Redis;

namespace Example1;

public interface IRedisService
{
    IDatabase GetDatabase();

    /// <summary>
    /// Execute an arbitrary command against the server; this is primarily intended for executing modules,
    /// but may also be used to provide access to new features that lack a direct API.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="args">The arguments to pass for the command.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>A dynamic representation of the command's result.</returns>
    /// <remarks>This API should be considered an advanced feature; inappropriate use can be harmful.</remarks>
    Task<RedisResult> ExecuteAsync(string command, CommandFlags flags = CommandFlags.None, params object[] args);

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

    /// <summary>
    /// Returns all fields and values of the hash stored at key.
    /// <seealso href="https://redis.io/commands/hgetall"/>
    /// </summary>
    /// <param name="key">The key of the hash to get all entries from.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>List of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
    Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

    /// <summary>
    /// Returns the value associated with field in the hash stored at key.
    /// </summary>
    /// <param name="key">The key of the hash.</param>
    /// <param name="hashField">The field in the hash to get.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <returns>The value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
    /// <remarks><seealso href="https://redis.io/commands/hget"/></remarks>
    Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

    /// <summary>
    /// Sets the specified fields to their respective values in the hash stored at key.
    /// This command overwrites any specified fields that already exist in the hash, leaving other unspecified fields untouched.
    /// If key does not exist, a new key holding a hash is created.
    /// </summary>
    /// <param name="key">The key of the hash.</param>
    /// <param name="hashFields">The entries to set in the hash.</param>
    /// <param name="flags">The flags to use for this operation.</param>
    /// <remarks><seealso href="https://redis.io/commands/hmset"/></remarks>
    Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None);
}