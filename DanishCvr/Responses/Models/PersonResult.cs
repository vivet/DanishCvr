using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Person Result.
/// </summary>
public class PersonResult : BasePerson
{
    /// <summary>
    /// Name.
    /// </summary>
    public virtual PersonName Name { get; set; }

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
    /// Errors.
    /// </summary>
    public virtual Errors Errors { get; set; }

    /// <summary>
    /// Companies.
    /// </summary>
    public virtual IEnumerable<PersonCompany> Companies { get; set; } = new List<PersonCompany>();
}