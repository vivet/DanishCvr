namespace DanishCvr.Responses.Models;

/// <summary>
/// Person Search Result.
/// </summary>
public class PersonSearchResult : BasePerson
{
    /// <summary>
    /// Name.
    /// </summary>
    public virtual PersonName Name { get; set; }

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
}