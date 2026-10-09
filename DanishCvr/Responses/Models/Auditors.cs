using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Auditors.
/// </summary>
public class Auditors
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Auditor Current { get; set; }

    /// <summary>
    /// Historic Auditors.
    /// </summary>
    public virtual IEnumerable<Auditor> HistoricAuditors { get; set; } = new List<Auditor>();
}