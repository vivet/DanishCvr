using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DanishCvr.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using DanishCvr.Responses.Models;

namespace Tests.DanishCvr;

public class BaseTests
{
    private const string DIRECTORY = ".test-output";

    protected IServiceProvider serviceProvider;

    [TestInitialize]
    public void TestSetup()
    {
        var configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var services = new ServiceCollection();

        services
            .AddSingleton<IConfiguration>(configurationRoot)
            .AddLogging(x => x
                .AddConfiguration(configurationRoot.GetSection("Logging"))
                .AddConsole());

        var logger = services
            .BuildServiceProvider()
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Test");

        services
            .AddSingleton(logger);

        services
            .AddDanishCvr();

        this.serviceProvider = services
            .BuildServiceProvider();
    }

    protected Task SaveJsonOrDefault(BaseDebugResult result, [CallerMemberName]string callerMethodName = "")
    {
        if (result == null)
        {
            return Task.CompletedTask;
        }

        if (!Directory.Exists(DIRECTORY))
        {
            Directory.CreateDirectory(DIRECTORY);
        }

        var contents = JsonConvert.SerializeObject(result, Formatting.Indented);

        return File.WriteAllTextAsync($"./{DIRECTORY}/{callerMethodName}.json", contents);
    }
}