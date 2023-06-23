using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Example1;

public class HashEntryTesting : IHashEntryTesting
{
    private readonly IRedisService _redisService;

    public HashEntryTesting(IRedisService redisService)
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
        var key = "testHash";

        Employee e007 = new Employee("007", "Davide Columbo", 100);

        HashEntry[] hashFields = new HashEntry[]
        {
            new HashEntry("syncId", Guid.NewGuid().ToString()),
            new HashEntry("007", JsonSerializer.Serialize(e007)),
        };
        
        Console.WriteLine($"{Environment.NewLine}Cache command: HMSET {key} via HashSetAsync()");
        await _redisService.HashSetAsync(key, hashFields, TimeSpan.FromSeconds(3));

        Console.WriteLine($"{Environment.NewLine}Cache command: HGET {key} Hash Field: syncId via HashGetAsync()");
        RedisValue syncIddata = await _redisService.HashGetAsync(key, "syncId");
        string syncIdDataAsString = syncIddata.HasValue ? syncIddata.ToString() : string.Empty;
        Console.WriteLine($"Cache response: {syncIdDataAsString}");

        Console.WriteLine($"{Environment.NewLine}Cache command: HGET {key} Hash Field: 007 via HashGetAsync()");
        RedisValue bonddata = await _redisService.HashGetAsync(key, "007");
        string bonddataAsString = bonddata.HasValue ? bonddata.ToString() : string.Empty;
        Console.WriteLine($"Cache response: {bonddataAsString}");

        Employee? d007 = JsonSerializer.Deserialize<Employee>(bonddataAsString);
        Console.WriteLine(d007 != null ? "Employee deserialized" : "Nothing deserialized");

        Console.WriteLine($"{Environment.NewLine}Cache command: HGETALL {key} via HashGetAllAsync()");
        HashEntry[] allData = await _redisService.HashGetAllAsync(key);

        foreach (HashEntry entry in allData)
        {
            Console.WriteLine($"Name: {entry.Name}  Value: {entry.Value}");
        }

        Console.WriteLine("Waiting 4 seconds to see if hash expires...");
        await Task.Delay(TimeSpan.FromSeconds(4));

        Console.WriteLine($"{Environment.NewLine}Cache command: HGETALL {key} via HashGetAllAsync()");
        allData = await _redisService.HashGetAllAsync(key);

        if (allData.Length > 0)
        {
            Console.WriteLine("BAD! Cache did NOT expire!");
        }
        else
        {
            Console.WriteLine("GOOD!  Cache expired!");
        }



    }
}