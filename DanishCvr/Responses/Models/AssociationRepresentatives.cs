using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Association Representatives.
/// </summary>
public class AssociationRepresentatives
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<AssociationRepresentative> Current { get; set; } = new List<AssociationRepresentative>();

    /// <summary>
    /// Historic Association Representatives.
    /// </summary>
    public virtual IEnumerable<AssociationRepresentative> HistoricAssociationRepresentatives { get; set; } = new List<AssociationRepresentative>();
}