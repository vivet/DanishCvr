using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Authorized Signatories.
/// </summary>
public class AuthorizedSignatories
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<AuthorizedSignatory> Current { get; set; } = new List<AuthorizedSignatory>();

    /// <summary>
    /// Historic Authorized Signatories.
    /// </summary>
    public virtual IEnumerable<AuthorizedSignatory> HistoricAuthorizedSignatories { get; set; } = new List<AuthorizedSignatory>();
}