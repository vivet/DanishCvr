using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Deltager Relation.
/// </summary>
public class DeltagerRelation
{
    /// <summary>
    /// Deltager.
    /// </summary>
    public virtual VrDeltager Deltager { get; set; } = new();

    /// <summary>
    /// kontor Steder.
    /// </summary>
    public virtual IEnumerable<KontorSted> KontorSteder { get; set; } = new List<KontorSted>();

    /// <summary>
    /// Organisationer.
    /// </summary>
    public virtual IEnumerable<Organisation> Organisationer { get; set; } = new List<Organisation>();
}