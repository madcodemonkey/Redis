using Microsoft.Extensions.Configuration;

namespace Example1;

/// <summary>
/// Uses CacheStringService 
/// </summary>
public class CacheStringServiceTesting : ICacheStringServiceTesting
{
    private readonly ICacheStringService _cacheService;

    public CacheStringServiceTesting(ICacheStringService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Work
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task WorkAsync(IConfiguration config)
    {
        var key = "testHash";


        Console.WriteLine($"{Environment.NewLine}Getting when not present...expecting Davide Columbo");
        var result1 =  await _cacheService.GetAsync<Employee>(key, TimeSpan.FromSeconds(5), GetEmployee1Async);
        Console.WriteLine($"{Environment.NewLine}Result: {result1}");

        Console.WriteLine($"{Environment.NewLine}Getting from cache present...expecting Davide Columbo");
        var result2 = await _cacheService.GetAsync<Employee>(key, TimeSpan.FromSeconds(5), GetEmployee2Async);
        Console.WriteLine($"{Environment.NewLine}Result: {result2}");

        Console.WriteLine("Waiting 12 seconds to see if memory cache expires...");
        await Task.Delay(TimeSpan.FromSeconds(12));

        Console.WriteLine($"{Environment.NewLine}Cache should be gone...expecting new Get to return Roger Moore");
        var result3 = await _cacheService.GetAsync<Employee>(key, TimeSpan.FromSeconds(5), GetEmployee2Async);
        Console.WriteLine($"{Environment.NewLine}Result: {result3}");

    }

    private async Task<Employee?> GetEmployee1Async()
    {
        Employee e007 = new Employee("007", "Davide Columbo", 45);

        return  await Task.FromResult(e007);
    }

    private async Task<Employee?> GetEmployee2Async()
    {
        Employee e007 = new Employee("007", "Roger Moore", 67);

        return await Task.FromResult(e007);
    }
}