using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Managers.
/// </summary>
public class Managers
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<Manager> Current { get; set; }

    /// <summary>
    /// Historic Managers.
    /// </summary>
    public virtual IEnumerable<Manager> HistoricManagers { get; set; } = new List<Manager>();
}