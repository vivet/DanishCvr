using System.Collections.Concurrent;

namespace DanishCvr.Models.Debug;

/// <summary>
/// Debug Hoved Type.
/// </summary>
public class DebugHovedType
{
    /// <summary>
    /// Type.
    /// </summary>
    public virtual string Type { get; set; }

    /// <summary>
    /// Count.
    /// </summary>
    public virtual int Count { get; set; }

    /// <summary>
    /// Attributes.
    /// </summary>
    public virtual ConcurrentBag<DebugAttribute> Attributes { get; set; } = [];
}