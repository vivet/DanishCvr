using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Anti Money Laundering Appointees.
/// </summary>
public class AntiMoneyLaunderingAppointees
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual AntiMoneyLaunderingAppointee Current { get; set; }

    /// <summary>
    /// Historic Anti Money Laundering Appointees.
    /// </summary>
    public virtual IEnumerable<AntiMoneyLaunderingAppointee> HistoricAntiMoneyLaunderingAppointees { get; set; } = new List<AntiMoneyLaunderingAppointee>();
}