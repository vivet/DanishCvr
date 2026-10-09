using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Executives.
/// </summary>
public class RelationExecutives
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationExecutive Current { get; set; }

    /// <summary>
    /// Historic Executives.
    /// </summary>
    public virtual IEnumerable<RelationExecutive> HistoricExecutives { get; set; } = new List<RelationExecutive>();
}