using System;
using DanishCvr.Responses.Models.Enums;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Base Production Unit (abstract).
/// </summary>
public abstract class BaseProductionUnit
{
    /// <summary>
    /// Production Unit Number.
    /// </summary>
    public virtual string ProductionUnitNumber { get; set; }

    /// <summary>
    /// Entity Type.
    /// </summary>
    public virtual EntityType EntityType { get; set; }

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
}