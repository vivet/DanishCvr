namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit.
/// </summary>
public class ProductionUnitResult : BaseProductionUnit
{
    /// <summary>
    /// Names.
    /// </summary>
    public virtual Names Names { get; set; }

    /// <summary>
    /// Addresses.
    /// </summary>
    public virtual Addresses Addresses { get; set; }

    /// <summary>
    /// Phone Numbers.
    /// </summary>
    public virtual PhoneNumbers PhoneNumbers { get; set; }

    /// <summary>
    /// Fax Numbers.
    /// </summary>
    public virtual FaxNumbers FaxNumbers { get; set; }

    /// <summary>
    /// Email Addresses.
    /// </summary>
    public virtual EmailAddresses EmailAddresses { get; set; }

    /// <summary>
    /// Industry.
    /// </summary>
    public virtual Industries Industry { get; set; }

    /// <summary>
    /// Employees.
    /// </summary>
    public virtual Employeeses Employees { get; set; }

    /// <summary>
    /// Companies.
    /// </summary>
    public virtual ProductionUnitCompanies Companies { get; set; }

    /// <summary>
    /// Status.
    /// </summary>
    public virtual StatusesSimple Status { get; set; }

    /// <summary>
    /// Is Headuarters.
    /// </summary>
    public virtual bool? IsHeaduarters { get; set; }

    /// <summary>
    /// Is Supporting Unit.
    /// </summary>
    public virtual bool? IsSupportingUnit { get; set; }

    /// <summary>
    /// Is Temporary.
    /// </summary>
    public virtual bool? IsTemporary { get; set; }

    /// <summary>
    /// Has Confidentiality.
    /// </summary>
    public virtual bool? HasConfidentiality { get; set; }

    /// <summary>
    /// Is Protected From Advertisement.
    /// </summary>
    public virtual bool? IsProtectedFromAdvertisement { get; set; }

    /// <summary>
    /// Errors.
    /// </summary>
    public virtual Errors Errors { get; set; }
}