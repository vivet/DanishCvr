using Newtonsoft.Json;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Base Debug Result (abstract).
/// </summary>
public abstract class BaseDebugResult
{
    /// <summary>
    /// Raw Json.
    /// Only populated in DEBUG mode.
    /// </summary>
    [JsonIgnore]
    public virtual string RawJson { get; set; }
}