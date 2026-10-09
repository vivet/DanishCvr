using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Managers.
/// </summary>
public class RelationManagers
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationManager Current { get; set; }

    /// <summary>
    /// Historic Managers.
    /// </summary>
    public virtual IEnumerable<RelationManager> HistoricManagers { get; set; } = new List<RelationManager>();
}