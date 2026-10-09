using DanishCvr.Responses.Models.Enums;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Base Relation Role (abstract).
/// </summary>
public abstract class BaseRelationRole : DateAndPeriod
{
    /// <summary>
    /// Entity Type.
    /// </summary>
    public virtual EntityType EntityType { get; set; }
}