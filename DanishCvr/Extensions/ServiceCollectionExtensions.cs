using System;
using System.Diagnostics;
using Elasticsearch.Net;
using DanishCvr.Extensions.Serialization;
using DanishCvr.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Nest;

namespace DanishCvr.Extensions;

/// <summary>
/// Service Collection Extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string HOST = "http://distribution.virk.dk/";

    /// <summary>
    /// Add Danish Cvr.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddDanishCvr(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services
            .AddConfigOptions<DanishCvrOptions>(DanishCvrOptions.SECTION_NAME, out var options);

        var connectionPool = new SingleNodeConnectionPool(new Uri(HOST));
        var connectionSettings = new ConnectionSettings(connectionPool, (_, _) => new CustomElasticSearchSerializer());

        connectionSettings
            .BasicAuthentication(options.Username, options.Password)
            .RequestTimeout(options.RequestTimeout)
            .ThrowExceptions();

        if (Debugger.IsAttached)
        {
            connectionSettings
                .DisableDirectStreaming()
                .PrettyJson();
        }

        services
            .AddSingleton(_ => new ElasticClient(connectionSettings));

        services
            .AddSingleton<IDanishCvrService, DanishCvrService>()
            .AddSingleton<IDanishCvrBatchService, DanishCvrBatchService>();

        return services;
    }

    private static IServiceCollection AddConfigOptions<TOption>(this IServiceCollection services, string name, out TOption options)
        where TOption : class, new()
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        var provider = services.BuildServiceProvider();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var section = configuration.GetSection(name);

        options = section.Get<TOption>() ?? new TOption();

        services
            .AddSingleton(options)
            .Configure<TOption>(section);

        return services;
    }
}