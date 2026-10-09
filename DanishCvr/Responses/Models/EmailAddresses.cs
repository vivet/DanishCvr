using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Email Addresses.
/// </summary>
public class EmailAddresses
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual EmailAddress Current { get; set; }

    /// <summary>
    /// Historic Email Addresses.
    /// </summary>
    public virtual IEnumerable<EmailAddress> HistoricEmailAddresses { get; set; } = new List<EmailAddress>();
}