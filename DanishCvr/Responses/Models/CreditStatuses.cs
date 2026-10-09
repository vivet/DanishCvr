using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Credit Statuses.
/// </summary>
public class CreditStatuses
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual CreditStatus Current { get; set; }

    /// <summary>
    /// Historic Statuses.
    /// </summary>
    public virtual IEnumerable<CreditStatus> HistoricStatuses { get; set; } = new List<CreditStatus>();
}