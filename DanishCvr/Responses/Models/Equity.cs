using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Ownership.
/// </summary>
public class Equity
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Share Current { get; set; }

    /// <summary>
    /// Historic Values.
    /// </summary>
    public virtual IEnumerable<Share> HistoricValues { get; set; } = new List<Share>();
}