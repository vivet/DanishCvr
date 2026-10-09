using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Auditors.
/// </summary>
public class RelationAuditors
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationAuditor Current { get; set; }

    /// <summary>
    /// Historic Auditors.
    /// </summary>
    public virtual IEnumerable<RelationAuditor> HistoricAuditors { get; set; } = new List<RelationAuditor>();
}