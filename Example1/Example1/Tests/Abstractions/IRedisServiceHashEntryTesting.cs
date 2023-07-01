using Microsoft.Extensions.Configuration;

namespace Example1;

public interface IRedisServiceHashEntryTesting
{
    Task WorkAsync(IConfiguration config);
}