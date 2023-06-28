using Microsoft.Extensions.Configuration;

namespace Example1;

public interface ICacheHashEntryTesting
{
    Task WorkAsync(IConfiguration config);
}