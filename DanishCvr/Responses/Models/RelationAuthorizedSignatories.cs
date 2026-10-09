using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Authorized Signatories.
/// </summary>
public class RelationAuthorizedSignatories
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationAuthorizedSignatory Current { get; set; }

    /// <summary>
    /// Historic Authorized Signatories.
    /// </summary>
    public virtual IEnumerable<RelationAuthorizedSignatory> HistoricAuthorizedSignatories { get; set; } = new List<RelationAuthorizedSignatory>();
}