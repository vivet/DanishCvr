using System.Collections.Generic;
using DanishCvr.Types;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Financial Years.
/// </summary>
public class FinancialYears
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual FinancialYear Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual FinancialYear Latest { get; set; }

    /// <summary>
    /// First Financial Year.
    /// </summary>
    public virtual Period FirstFinancialYear { get; set; }

    /// <summary>
    /// Ongoing Transition Period.
    /// </summary>
    public virtual Period OngoingTransitionPeriod { get; set; }

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }

    /// <summary>
    /// HistoricFinancialYears.
    /// </summary>
    public virtual IEnumerable<FinancialYear> HistoricFinancialYears { get; set; }
}