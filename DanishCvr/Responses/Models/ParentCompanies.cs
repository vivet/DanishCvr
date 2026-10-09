using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Parent Companies.
/// </summary>
public class ParentCompanies
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual ParentCompany Current { get; set; }

    /// <summary>
    /// Historic Parent Companies.
    /// </summary>
    public virtual IEnumerable<ParentCompany> HistoricParentCompanies { get; set; } = new List<ParentCompany>();
}