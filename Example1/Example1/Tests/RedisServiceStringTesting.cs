using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Example1;

/// <summary>
/// This test is based on Microsoft docs <see href="https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-dotnet-core-quickstart"/> and
/// on github <see href="https://github.com/Azure-Samples/azure-cache-redis-samples/tree/main/quickstart/dotnet-core"/>.  You'll notice that the use
/// a connection object and make reference to using Polly.  Most people don't do that, but you might need something similar.
/// </summary>
public class RedisServiceStringTesting : IRedisServiceStringTesting
{
    private readonly IRedisService _redisService;

    public RedisServiceStringTesting(IRedisService redisService)
    {
        _redisService = redisService;
    }

    /// <summary>
    /// Work
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task WorkAsync(IConfiguration config)
    {
        var expiry = TimeSpan.FromMinutes(2);

        // Simple PING command
        Console.WriteLine($"{Environment.NewLine}Cache command: PING");
        RedisResult pingResult = await _redisService.ExecuteAsync("PING");
        Console.WriteLine($"Cache response: {pingResult}");

        // Simple get and put of integral data types into the cache
        string key = "Message";
        string value = "Hello! The cache is working from a .NET console app!";

        Console.WriteLine($"{Environment.NewLine}Cache command: GET {key} via StringGetAsync()");
        RedisValue getMessageResult = await _redisService.StringGetAsync(key);
        Console.WriteLine($"Cache response: {getMessageResult}");

        Console.WriteLine($"{Environment.NewLine}Cache command: SET {key} \"{value}\" via StringSetAsync()");
        bool stringSetResult = await _redisService.StringSetAsync(key, value, expiry);
        Console.WriteLine($"Cache response: {stringSetResult}");

        Console.WriteLine($"{Environment.NewLine}Cache command: GET {key} via StringGetAsync()");
        getMessageResult = await _redisService.StringGetAsync(key); 
        Console.WriteLine($"Cache response: {getMessageResult}");

        // Store serialized object to cache
        Employee e007 = new Employee("007", "Davide Columbo", 100);
        stringSetResult = await _redisService.StringSetAsync("e007", JsonSerializer.Serialize(e007), expiry);
        Console.WriteLine($"{Environment.NewLine}Cache response from storing serialized Employee object: {stringSetResult}");

        // Retrieve serialized object from cache
        getMessageResult = await _redisService.StringGetAsync("e007");
        Employee e007FromCache = JsonSerializer.Deserialize<Employee>(getMessageResult);
        Console.WriteLine($"Deserialized Employee .NET object:");
        Console.WriteLine($"- Employee.Name : {e007FromCache.Name}");
        Console.WriteLine($"- Employee.Id   : {e007FromCache.Id}");
        Console.WriteLine($"- Employee.Age  : {e007FromCache.Age}{Environment.NewLine}");
    }
}