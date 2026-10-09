using System.Collections.Concurrent;

namespace DanishCvr.Models.Debug;

/// <summary>
/// Debug Information.
/// </summary>
public class DebugInformation
{
    /// <summary>
    /// Hoved Typer.
    /// </summary>
    public virtual ConcurrentBag<DebugHovedType> HovedTyper { get; set; } = [];

    /// <summary>
    /// Attributes.
    /// </summary>
    public virtual ConcurrentBag<DebugAttribute> Attributes { get; set; } = [];
}