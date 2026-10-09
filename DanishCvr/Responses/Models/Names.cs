using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Names.
/// </summary>
public class Names
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Name Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual Name Latest { get; set; }

    /// <summary>
    /// Historic Names.
    /// </summary>
    public virtual IEnumerable<Name> HistoricNames { get; set; } = new List<Name>();
}