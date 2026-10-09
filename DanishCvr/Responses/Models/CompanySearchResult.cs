namespace DanishCvr.Responses.Models;

/// <summary>
/// Company Search Result.
/// </summary>
public class CompanySearchResult : BaseCompany
{
    /// <summary>
    /// Name.
    /// </summary>
    public virtual Name Name { get; set; }

    /// <summary>
    /// Address.
    /// </summary>
    public virtual Address Address { get; set; }

    /// <summary>
    /// Phone Number.
    /// </summary>
    public virtual PhoneNumber PhoneNumber { get; set; }

    /// <summary>
    /// Fax Number.
    /// </summary>
    public virtual FaxNumber FaxNumber { get; set; }

    /// <summary>
    /// Email Address.
    /// </summary>
    public virtual EmailAddress EmailAddress { get; set; }

    /// <summary>
    /// Website.
    /// </summary>
    public virtual Website Website { get; set; }

    /// <summary>
    /// Purpose.
    /// </summary>
    public virtual Purpose Purpose { get; set; }

    /// <summary>
    /// Industry.
    /// </summary>
    public virtual Industry Industry { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    public virtual BusinessType Type { get; set; }

    /// <summary>
    /// Status.
    /// </summary>
    public virtual Status Status { get; set; }

    /// <summary>
    /// Employees.
    /// </summary>
    public virtual Employees Employees { get; set; }

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
    /// Is Active.
    /// </summary>
    public virtual bool IsActive { get; set; }
}