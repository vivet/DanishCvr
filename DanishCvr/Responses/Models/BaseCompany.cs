using System;
using DanishCvr.Responses.Models.Enums;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Base Company (abstract).
/// </summary>
public abstract class BaseCompany
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }

    /// <summary>
    /// Entity Type.
    /// </summary>
    public virtual EntityType EntityType { get; set; }

    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; }

    /// <summary>
    /// Updated At.
    /// </summary>
    public virtual DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Founded At.
    /// </summary>
    public virtual DateOnly? FoundedAt { get; set; }

    /// <summary>
    /// Dissolved At.
    /// </summary>
    public virtual DateOnly? DissolvedAt { get; set; }

    /// <summary>
    /// Is Protected From Advertisement.
    /// </summary>
    public virtual bool? IsProtectedFromAdvertisement { get; set; }
}