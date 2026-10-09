using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit Companies.
/// </summary>
public class ProductionUnitCompanies
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<ProductionUnitCompany> Current { get; set; } = new List<ProductionUnitCompany>();

    /// <summary>
    /// Historic Companies.
    /// </summary>
    public virtual IEnumerable<ProductionUnitCompany> HistoricCompanies { get; set; } = new List<ProductionUnitCompany>();
}