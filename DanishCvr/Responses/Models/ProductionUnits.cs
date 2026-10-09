using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Websites.
/// </summary>
public class ProductionUnits
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<ProductionUnitId> Current { get; set; } = new List<ProductionUnitId>();

    /// <summary>
    /// Historic Production Units.
    /// </summary>
    public virtual IEnumerable<ProductionUnitId> HistoricProductionUnits { get; set; } = new List<ProductionUnitId>();
}