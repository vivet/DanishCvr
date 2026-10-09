using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Medlems Data.
/// </summary>
public class MedlemsData
{
    /// <summary>
    /// Attributter
    /// </summary>
    public virtual IEnumerable<Attributter> Attributter { get; set; } = new List<Attributter>();
}