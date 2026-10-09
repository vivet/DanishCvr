using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Alternative Namesses.
/// </summary>
public class AlternativeNamesses
{
    /// <summary>
    /// Names.
    /// </summary>
    public virtual IEnumerable<Name> Names { get; set; } = new List<Name>();

    /// <summary>
    /// Historic Names.
    /// </summary>
    public virtual IEnumerable<Name> HistoricNames { get; set; } = new List<Name>();
}