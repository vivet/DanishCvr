using DanishCvr.Responses.Models.Enums;
using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Person Company.
/// </summary>
public class PersonCompany
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    public virtual Name Name { get; set; }

    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; }

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// Active Roles.
    /// </summary>
    public virtual IEnumerable<RelationType> ActiveRoles { get; set; } = new List<RelationType>();
}