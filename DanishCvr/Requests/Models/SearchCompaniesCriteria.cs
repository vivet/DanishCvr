using System.Collections.Generic;
using DanishCvr.Types;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Search Companies Criteria.
/// </summary>
public class SearchCompaniesCriteria
{
    /// <summary>
    /// Names.
    /// </summary>
    public virtual NamesWithAlternative Names { get; set; } = new();

    /// <summary>
    /// Business Type Codes.
    /// </summary>
    public virtual IEnumerable<string> BusinessTypeCodes { get; set; } = new List<string>();

    /// <summary>
    /// Industry Codes.
    /// </summary>
    public virtual IEnumerable<string> IndustryCodes { get; set; } = new List<string>();

    /// <summary>
    /// Statuses.
    /// </summary>
    public virtual IEnumerable<string> Statuses { get; set; } = new List<string>();

    /// <summary>
    /// Founded At.
    /// </summary>
    public virtual Period FoundedAt { get; set; } = new();

    /// <summary>
    /// Dissolved At.
    /// </summary>
    public virtual Period DissolvedAt { get; set; } = new();

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

    /// <summary>
    /// Is Social Economic.
    /// </summary>
    public virtual bool? IsSocialEconomic { get; set; }

    /// <summary>
    /// Is Governmental.
    /// </summary>
    public virtual bool? IsGovernmental { get; set; }

    /// <summary>
    /// Is Protected From Advertisment.
    /// </summary>
    public virtual bool? IsProtectedFromAdvertisment { get; set; }
}