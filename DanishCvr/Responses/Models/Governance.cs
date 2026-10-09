using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Governance.
/// </summary>
public class Governance
{
    /// <summary>
    /// Oversight.
    /// </summary>
    public virtual Oversight Oversight { get; set; }

    /// <summary>
    /// Board Members.
    /// </summary>
    public virtual BoardMembers BoardMembers { get; set; }

    /// <summary>
    /// Association Representatives.
    /// </summary>
    public virtual AssociationRepresentatives AssociationRepresentatives { get; set; }

    /// <summary>
    /// Founders.
    /// </summary>
    public virtual IEnumerable<Founder> Founders { get; set; } = new List<Founder>();
}