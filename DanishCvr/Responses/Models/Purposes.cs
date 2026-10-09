using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Purposes.
/// </summary>
public class Purposes
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Purpose Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual Purpose Latest { get; set; }

    /// <summary>
    /// Financial Purpose.
    /// </summary>
    public virtual FinancialPurpose FinancialPurpose { get; set; }

    /// <summary>
    /// Historic Purposes.
    /// </summary>
    public virtual IEnumerable<Purpose> HistoricPurposes { get; set; } = new List<Purpose>();
}