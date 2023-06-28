using Microsoft.Extensions.Configuration;

namespace Example1;

public interface ICacheStringTesting
{
    Task WorkAsync(IConfiguration config);
}