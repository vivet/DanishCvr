using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Addresses.
/// </summary>
public class Addresses
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Address Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual Address Latest { get; set; }

    /// <summary>
    /// Historic Addresses.
    /// </summary>
    public virtual IEnumerable<Address> HistoricAddresses { get; set; } = new List<Address>();
}