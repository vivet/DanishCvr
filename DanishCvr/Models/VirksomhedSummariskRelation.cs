using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Virksomhed Summarisk Relation.
/// </summary>
public class VirksomhedSummariskRelation
{
    /// <summary>
    /// Virksomhed.
    /// </summary>
    public virtual VirksomhedSummarisk Virksomhed { get; set; }

    /// <summary>
    /// Organisationer.
    /// </summary>
    public virtual IEnumerable<Organisation> Organisationer { get; set; } = new List<Organisation>();
}