using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Business Types.
/// </summary>
public class BusinessTypes
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual BusinessType Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual BusinessType Latest { get; set; }

    /// <summary>
    /// Is Governmental.
    /// </summary>
    public virtual bool? IsGovernmental { get; set; }

    /// <summary>
    /// Is Publicly Listed.
    /// </summary>
    public virtual bool? IsPubliclyListed { get; set; }

    /// <summary>
    /// Is Social Economic.
    /// </summary>
    public virtual bool? IsSocialEconomic { get; set; }

    /// <summary>
    /// Is Certified Auditor.
    /// </summary>
    public virtual bool? IsCertifiedAuditor { get; set; }

    /// <summary>
    /// Historic Types.
    /// </summary>
    public virtual IEnumerable<BusinessType> HistoricTypes { get; set; } = new List<BusinessType>();
}