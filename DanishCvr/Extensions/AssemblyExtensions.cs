using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DanishCvr.Extensions;

/// <summary>
/// Assembly Extensions.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Get Resource.
    /// </summary>
    /// <typeparam name="T">The resource </typeparam>
    /// <param name="assembly">The <see cref="Assembly"/>.</param>
    /// <param name="name">The name.</param>
    /// <returns>The json deserialized to type T.</returns>
    public static T GetJsonResource<T>(this Assembly assembly, string name)
    {
        var resourceContents = assembly
            .GetResource(name)
            .GetAwaiter()
            .GetResult();

        return JsonConvert.DeserializeObject<T>(resourceContents);
    }

    /// <summary>
    /// Get Resource.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The resource contents.</returns>
    public static async Task<string> GetResource(this Assembly assembly, string name, CancellationToken cancellationToken = default)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        await using var stream = assembly.GetManifestResourceStream(name);

        if (stream == null)
        {
            throw new NullReferenceException(nameof(stream));
        }

        using var reader = new StreamReader(stream);

        return await reader
            .ReadToEndAsync(cancellationToken);
    }
}