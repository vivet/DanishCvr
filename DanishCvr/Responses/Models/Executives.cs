using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Executives.
/// </summary>
public class Executives
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<Executive> Current { get; set; } = new List<Executive>();

    /// <summary>
    /// Historic Executives.
    /// </summary>
    public virtual IEnumerable<Executive> HistoricExecutives { get; set; } = new List<Executive>();
}