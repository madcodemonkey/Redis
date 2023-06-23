using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Example1;

public class StringTesting : IStringTesting
{
    private readonly IRedisService _redisService;

    public StringTesting(IRedisService redisService)
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
        string prefix = string.Empty;
        
        // Simple PING command
        Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: PING");
        RedisResult pingResult = await _redisService.ExecuteAsync("PING");
        Console.WriteLine($"{prefix}: Cache response: {pingResult}");

        // Simple get and put of integral data types into the cache
        string key = "Message";
        string value = "Hello! The cache is working from a .NET console app!";

        Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: GET {key} via StringGetAsync()");
        RedisValue getMessageResult = await _redisService.StringGetAsync(key);
        Console.WriteLine($"{prefix}: Cache response: {getMessageResult}");

        Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: SET {key} \"{value}\" via StringSetAsync()");
        bool stringSetResult = await _redisService.StringSetAsync(key, value);
        Console.WriteLine($"{prefix}: Cache response: {stringSetResult}");

        Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: GET {key} via StringGetAsync()");
        getMessageResult = await _redisService.StringGetAsync(key); 
        Console.WriteLine($"{prefix}: Cache response: {getMessageResult}");

        // Store serialized object to cache
        Employee e007 = new Employee("007", "Davide Columbo", 100);
        stringSetResult = await _redisService.StringSetAsync("e007", JsonSerializer.Serialize(e007));
        Console.WriteLine($"{Environment.NewLine}{prefix}: Cache response from storing serialized Employee object: {stringSetResult}");

        // Retrieve serialized object from cache
        getMessageResult = await _redisService.StringGetAsync("e007");
        Employee e007FromCache = JsonSerializer.Deserialize<Employee>(getMessageResult);
        Console.WriteLine($"{prefix}: Deserialized Employee .NET object:{Environment.NewLine}");
        Console.WriteLine($"{prefix}: Employee.Name : {e007FromCache.Name}");
        Console.WriteLine($"{prefix}: Employee.Id   : {e007FromCache.Id}");
        Console.WriteLine($"{prefix}: Employee.Age  : {e007FromCache.Age}{Environment.NewLine}");
    }
}