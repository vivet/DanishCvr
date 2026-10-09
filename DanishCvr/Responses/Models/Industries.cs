using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Industries
/// </summary>
public class Industries
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Industry Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual Industry Latest { get; set; }

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }

    /// <summary>
    /// Historic Primaries.
    /// </summary>
    public virtual IEnumerable<Industry> HistoricIndustries { get; set; } = new List<Industry>();

    /// <summary>
    /// Secondary Industries.
    /// </summary>
    public virtual IEnumerable<Industry> SecondaryIndustries { get; set; } = new List<Industry>();

    /// <summary>
    /// Historic Secondary Industries.
    /// </summary>
    public virtual IEnumerable<Industry> HistoricSecondaryIndustries { get; set; } = new List<Industry>();
}