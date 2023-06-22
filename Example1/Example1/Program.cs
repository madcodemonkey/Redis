using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using Example1;
using Microsoft.Extensions.Logging;

IConfiguration config = SetupConfiguration();
IServiceProvider serviceProvider = RegisterDependencies(config);




var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
logger.LogWarning("This is a warning");




static IServiceProvider RegisterDependencies(IConfiguration configuration)
{
    // ServiceCollection:  Requires Microsoft.Extensions.DependencyInjection
    var collection = new ServiceCollection();
    collection.AddSingleton(configuration);

    var settings = new ExampleSettings()
    {
        RedisConnection = configuration["RedisConnection"]
    };

    collection.AddSingleton(settings);
    collection.AddSingleton<IRedisService, RedisService>();

    collection.AddLogging(configure => configure.AddConsole());


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