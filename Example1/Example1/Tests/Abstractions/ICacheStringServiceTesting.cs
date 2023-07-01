using Microsoft.Extensions.Configuration;

namespace Example1;

public interface ICacheStringServiceTesting
{
    Task WorkAsync(IConfiguration config);
}