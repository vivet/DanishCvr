using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Kontor Sted.
/// </summary>
public class KontorSted
{
    /// <summary>
    /// P Enhed.
    /// </summary>
    public virtual PEnhed PEnhed { get; set; } = new();

    /// <summary>
    /// Attributter.
    /// </summary>
    public virtual IEnumerable<Attributter> Attributter { get; set; } = new List<Attributter>();
}