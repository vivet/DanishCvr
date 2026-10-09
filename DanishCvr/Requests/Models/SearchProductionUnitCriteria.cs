using System.Collections.Generic;
using DanishCvr.Types;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Search Production Unit Criteria.
/// </summary>
public class SearchProductionUnitCriteria
{
    /// <summary>
    /// Names.
    /// </summary>
    public virtual Names Names { get; set; } = new();

    /// <summary>
    /// Industry Codes.
    /// </summary>
    public virtual IEnumerable<string> IndustryCodes { get; set; } = new List<string>();

    /// <summary>
    /// Statuses.
    /// </summary>
    public virtual IEnumerable<string> Statuses { get; set; } = new List<string>();

    /// <summary>
    /// Number Of Employees.
    /// </summary>
    public virtual Range NumberOfEmployees { get; set; } = new();

    /// <summary>
    /// Within.
    /// </summary>
    public virtual Within Within { get; set; }

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool? IsActive { get; set; }
}