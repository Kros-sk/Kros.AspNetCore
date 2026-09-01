using Microsoft.Extensions.Configuration;
using System.IO;

namespace Kros.AspNetCore.Tests;

internal static class TestsHelper
{
    internal static IConfiguration GetConfiguration()
        => new ConfigurationBuilder().AddJsonFile(Path.Combine("Extensions", "appsettings.configuration-test.json"))
            .Build();

    internal static IConfiguration GetSignalRBadConfiguration()
        => new ConfigurationBuilder().AddJsonFile(Path.Combine("Extensions", "appsettings.configuration-test-signalr-bad.json"))
            .Build();
}
