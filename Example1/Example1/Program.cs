using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Example1;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

IConfiguration config = SetupConfiguration();
IServiceProvider serviceProvider = RegisterDependencies(config);
ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    Console.WriteLine("------------------------------ Start Redis Service String test -------------------------------------");
    var stringTest = serviceProvider.GetRequiredService<IRedisServiceStringTesting>();
    await stringTest.WorkAsync(config);
    Console.WriteLine("------------------------------ End Redis Service String test -------------------------------------");


    Console.WriteLine("------------------------------ Start Redis Service HASH test -------------------------------------");
    var hashTest = serviceProvider.GetRequiredService<IRedisServiceHashEntryTesting>();
    await hashTest.WorkAsync(config);
    Console.WriteLine("------------------------------ End Redis Service HASH test -------------------------------------");

    Console.WriteLine("------------------------------ Start Memory CACHE with Redis: HASH test -------------------------------------");
    var cacheHashTest = serviceProvider.GetRequiredService<ICacheHashEntryTesting>();
    await cacheHashTest.WorkAsync(config);
    Console.WriteLine("------------------------------ End Memory CACHE with Redis: HASH test -------------------------------------");

    Console.WriteLine("------------------------------ Start Memory CACHE with Redis: STRING test -------------------------------------");
    var cacheStringTest = serviceProvider.GetRequiredService<ICacheStringServiceTesting>();
    await cacheStringTest.WorkAsync(config);
    Console.WriteLine("------------------------------ End Memory CACHE with Redis: STRING test -------------------------------------");


    
}
catch (Exception ex)
{
   logger.LogError(ex, "Something went wrong!");    
}


static IServiceProvider RegisterDependencies(IConfiguration configuration)
{
    // ServiceCollection:  Requires Microsoft.Extensions.DependencyInjection
    var collection = new ServiceCollection();
    collection.AddSingleton(configuration);

    var settings = new ExampleSettings()
    {
        RedisConnection = configuration["RedisConnection"]
    };

    collection.AddLogging(configure => configure.AddConsole());

    // Singletons
    collection.AddSingleton(settings);
    collection.AddSingleton<IRedisService, RedisService>();
    collection.AddSingleton<IMemoryCache, MemoryCache>();
    // Transient
    collection.AddTransient<ICacheHashEntryTesting, CacheHashSetServiceTesting>();
    collection.AddTransient<ICacheStringServiceTesting, CacheStringServiceTesting>();
    collection.AddTransient<ICacheStringService, CacheStringService>();
    collection.AddTransient<ICacheHashSetService, CacheHashSetService>();
    collection.AddTransient<IRedisServiceHashEntryTesting, RedisServiceHashEntryTesting>();
    collection.AddTransient<IRedisServiceStringTesting, RedisServiceStringTesting>();
    
    var serviceProvider = collection.BuildServiceProvider();
 
    return serviceProvider;
}


static IConfiguration SetupConfiguration()
{
    // AddJsonFile requires:    Microsoft.Extensions.Configuration.Json NuGet package
    // AddUserSecrets requires: Microsoft.Extensions.Configuration.UserSecrets NuGet package
    // IConfiguration requires: Microsoft.Extensions.Configuration NuGet package (pulled in by previous NuGet)
    // https://stackoverflow.com/a/46437144/97803
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddUserSecrets<Program>();  // this is optional if you don't plan on using secrets

    IConfiguration config = builder.Build();

    return config;
}