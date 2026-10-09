using System.Collections.Concurrent;

namespace DanishCvr.Models.Debug;

/// <summary>
/// Debug Attribute.
/// </summary>
public class DebugAttribute
{
    /// <summary>
    /// Value.
    /// </summary>
    public virtual string Value { get; set; }

    /// <summary>
    /// Count.
    /// </summary>
    public virtual int Count { get; set; }

    /// <summary>
    /// Sample Registration Numbers.
    /// </summary>
    public virtual ConcurrentBag<string> SampleRegistrationNumbers { get; set; } = [];
}