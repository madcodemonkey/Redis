using Microsoft.Extensions.Configuration;

namespace Example1;

public interface IRedisServiceStringTesting
{
    Task WorkAsync(IConfiguration config);
}