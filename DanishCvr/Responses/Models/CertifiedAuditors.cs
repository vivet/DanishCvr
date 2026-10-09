using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Certified Auditors.
/// </summary>
public class CertifiedAuditors
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<CertifiedAuditor> Current { get; set; } = new List<CertifiedAuditor>();

    /// <summary>
    /// Historic Certified Auditors.
    /// </summary>
    public virtual IEnumerable<CertifiedAuditor> HistoricCertifiedAuditors { get; set; } = new List<CertifiedAuditor>();
}