using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Example1;

public class StringTesting
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
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("dave.redis.cache.windows.net:6380,password=MyPassword,ssl=True,abortConnect=False");
        IDatabase db = redis.GetDatabase();

        //Employee e007 = new Employee("007", "Davide Columbo", 100);

        //HashEntry[] hashFields = new HashEntry[]
        //{
        //    new HashEntry("syncId", Guid.NewGuid().ToString()),
        //    new HashEntry("007", JsonSerializer.Serialize(e007)),
        //};

        //await db.HashSetAsync("testHash", hashFields);


        //RedisValue syncIddata = await db.HashGetAsync("testHash", "syncId");
        //string syncIdDataAsString = syncIddata.HasValue ? syncIddata.ToString() : string.Empty;

        //RedisValue bonddata = await db.HashGetAsync("testHash", "007");

        //string bonddataAsString = bonddata.HasValue ? bonddata.ToString() : string.Empty;
        //Employee? d007 = JsonSerializer.Deserialize<Employee>(bonddataAsString);

        HashEntry[]? allData = await db.HashGetAllAsync("testHash");
    }
}