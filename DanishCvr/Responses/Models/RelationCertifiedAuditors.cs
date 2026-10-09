using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Certified Auditors.
/// </summary>
public class RelationCertifiedAuditors
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationCertifiedAuditor Current { get; set; }

    /// <summary>
    /// Historic Certified Auditors.
    /// </summary>
    public virtual IEnumerable<RelationCertifiedAuditor> HistoricCertifiedAuditors { get; set; } = new List<RelationCertifiedAuditor>();
}