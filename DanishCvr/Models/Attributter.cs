using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Attributter.
/// </summary>
public class Attributter
{
    /// <summary>
    /// SekvensNr.
    /// </summary>
    public virtual int SekvensNr { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    public virtual string Type { get; set; }

    /// <summary>
    /// Vaerdi Type.
    /// </summary>
    public virtual string VaerdiType { get; set; }

    /// <summary>
    /// Vaerdier.
    /// </summary>
    public virtual IEnumerable<Vaerdier> Vaerdier { get; set; } = new List<Vaerdier>();
}