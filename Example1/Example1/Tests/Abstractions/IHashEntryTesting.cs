using Microsoft.Extensions.Configuration;

namespace Example1;

public interface IHashEntryTesting
{
    Task WorkAsync(IConfiguration config);
}