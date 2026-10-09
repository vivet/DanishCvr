using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Result.
/// </summary>
public class RelationDebugResult : BaseDebugResult
{
    /// <summary>
    /// Companies.
    /// </summary>
    public virtual IEnumerable<RelationResult> Companies { get; set; } = new List<RelationResult>();
}