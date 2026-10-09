using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Bilawsses.
/// </summary>
public class Bilawsses
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Bilaws Current { get; set; }

    /// <summary>
    /// Approval.
    /// </summary>
    public virtual BilawsApproval Approval { get; set; }

    /// <summary>
    /// Historic Bilaws.
    /// </summary>
    public virtual IEnumerable<Bilaws> HistoricBilaws { get; set; } = new List<Bilaws>();
}