using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Employeeses.
/// </summary>
public class Employeeses
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Employees Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual Employees Latest { get; set; }

    /// <summary>
    /// Historic Employment.
    /// </summary>
    public virtual IEnumerable<Employees> HistoricEmployees { get; set; } = new List<Employees>();
}