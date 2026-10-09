using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Statuses.
/// </summary>
public class Statuses
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Status Current { get; set; }

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// Credit Status.
    /// </summary>
    public virtual CreditStatuses CreditStatus { get; set; }

    /// <summary>
    /// Historic Statuses.
    /// </summary>
    public virtual IEnumerable<Status> HistoricStatuses { get; set; } = new List<Status>();
}