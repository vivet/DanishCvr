using System;
using DanishCvr.Responses.Models.Enums;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Base Person (abstract).
/// </summary>
public abstract class BasePerson
{
    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; }

    /// <summary>
    /// Title.
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// Entity Type.
    /// </summary>
    public virtual EntityType EntityType { get; set; }

    /// <summary>
    /// Updated At.
    /// </summary>
    public virtual DateTimeOffset? UpdatedAt { get; set; }
}