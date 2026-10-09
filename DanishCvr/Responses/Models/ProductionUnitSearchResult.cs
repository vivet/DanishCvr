namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit Search.
/// </summary>
public class ProductionUnitSearchResult : BaseProductionUnit
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
    /// Industry.
    /// </summary>
    public virtual Industry Industry { get; set; }

    /// <summary>
    /// Employees.
    /// </summary>
    public virtual Employees Employees { get; set; }

    /// <summary>
    /// Status.
    /// </summary>
    public virtual StatusesSimple Status { get; set; }

    /// <summary>
    /// Is Protected From Advertisement.
    /// </summary>
    public virtual bool? IsProtectedFromAdvertisement { get; set; }
}