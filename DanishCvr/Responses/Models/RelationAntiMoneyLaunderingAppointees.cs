using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Anti Money Laundering Appointees.
/// </summary>
public class RelationAntiMoneyLaunderingAppointees
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationAntiMoneyLaunderingAppointee Current { get; set; }

    /// <summary>
    /// Historic Anti Money Laundering Appointees.
    /// </summary>
    public virtual IEnumerable<RelationAntiMoneyLaunderingAppointee> HistoricAntiMoneyLaunderingAppointees { get; set; } = new List<RelationAntiMoneyLaunderingAppointee>();
}