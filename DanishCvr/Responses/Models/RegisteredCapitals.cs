using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Registered Capitals.
/// </summary>
public class RegisteredCapitals
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RegisteredCapital Current { get; set; }

    /// <summary>
    /// Is Partially Paid.
    /// </summary>
    public virtual bool? IsPartiallyPaid { get; set; }

    /// <summary>
    /// Historic Registered Capital.
    /// </summary>
    public virtual IEnumerable<RegisteredCapital> HistoricRegisteredCapitals { get; set; } = new List<RegisteredCapital>();
}