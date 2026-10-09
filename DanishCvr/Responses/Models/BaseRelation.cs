using DanishCvr.Responses.Models.Enums;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Base Relation (abstract).
/// </summary>
public abstract class BaseRelation : DateAndPeriod
{
    /// <summary>
    /// Entity Type.
    /// </summary>
    public virtual EntityType EntityType { get; set; }

    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; } 

    /// <summary>
    /// Name.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Email Address.
    /// </summary>
    public virtual string EmailAddress { get; set; }

    /// <summary>
    /// Phone Number.
    /// </summary>
    public virtual string PhoneNumber { get; set; }

    /// <summary>
    /// Address.
    /// </summary>
    public virtual Address Address { get; set; }
}