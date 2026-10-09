using DanishCvr.Responses.Models.Enums;
using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Result.
/// </summary>
public class RelationResult : BaseCompany
{
    /// <summary>
    /// Name.
    /// </summary>
    public virtual Name Name { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    public virtual BusinessType Type { get; set; }

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// Roles.
    /// </summary>
    public virtual RelationRoles Roles { get; set; } = new();

    /// <summary>
    /// Aktive Roles.
    /// </summary>
    public virtual IEnumerable<RelationType> ActiveRoles { get; set; } = new List<RelationType>();
}