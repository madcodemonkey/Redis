using Microsoft.Extensions.Configuration;

namespace Example1;

public interface IStringTesting
{
    Task WorkAsync(IConfiguration config);
}