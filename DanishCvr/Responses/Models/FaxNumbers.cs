using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Fax Numbers
/// </summary>
public class FaxNumbers
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual FaxNumber Current { get; set; }

    /// <summary>
    /// Historic Fax Numbers.
    /// </summary>
    public virtual IEnumerable<FaxNumber> HistoricFaxNumbers { get; set; } = new List<FaxNumber>();
}