using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Association Representatives.
/// </summary>
public class RelationAssociationRepresentatives
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationAssociationRepresentative Current { get; set; }

    /// <summary>
    /// Historic Association Representatives.
    /// </summary>
    public virtual IEnumerable<RelationAssociationRepresentative> HistoricAssociationRepresentatives { get; set; } = new List<RelationAssociationRepresentative>();
}