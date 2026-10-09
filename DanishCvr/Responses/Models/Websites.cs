using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Websites.
/// </summary>
public class Websites
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Website Current { get; set; }

    /// <summary>
    /// Historic Websites.
    /// </summary>
    public virtual IEnumerable<Website> HistoricWebsites { get; set; } = new List<Website>();
}