using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Ownership Specials.
/// </summary>
public class OwnershipSpecials
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual OwnershipSpecial Current { get; set; }

    /// <summary>
    /// Historic Values.
    /// </summary>
    public virtual IEnumerable<OwnershipSpecial> HistoricValues { get; set; } = new List<OwnershipSpecial>();
}