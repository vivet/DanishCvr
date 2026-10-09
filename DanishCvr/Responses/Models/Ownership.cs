using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Ownership.
/// </summary>
public class Ownership
{
    /// <summary>
    /// Has Share Classes.
    /// </summary>
    public virtual bool HasShareClasses { get; set; } = false;

    /// <summary>
    /// Has Only Under 5 Percent Ownerships.
    /// </summary>
    public virtual bool HasOnlyUnder5PercentOwnerships { get; set; } = false;

    /// <summary>
    /// Has Public Shareholder Registry.
    /// </summary>
    public virtual bool HasPublicShareholderRegistry { get; set; } = false;

    /// <summary>
    /// Beneficiary.
    /// </summary>
    public virtual Beneficiary Beneficiary { get; set; }

    /// <summary>
    /// Parent Company.
    /// </summary>
    public virtual ParentCompanies ParentCompany { get; set; }

    /// <summary>
    /// Legal Owners.
    /// </summary>
    public virtual IEnumerable<LegalOwner> LegalOwners { get; set; } = new List<LegalOwner>();

    /// <summary>
    /// Beneficial Owners.
    /// </summary>
    public virtual IEnumerable<BeneficialOwner> BeneficialOwners { get; set; } = new List<BeneficialOwner>();

    /// <summary>
    /// Mergers.
    /// </summary>
    public virtual IEnumerable<MergersAndSplits> Mergers { get; set; } = new List<MergersAndSplits>();

    /// <summary>
    /// Demergers.
    /// </summary>
    public virtual IEnumerable<MergersAndSplits> DeMergers { get; set; } = new List<MergersAndSplits>();
}