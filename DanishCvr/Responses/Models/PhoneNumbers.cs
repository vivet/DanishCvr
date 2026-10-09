using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Phone Numbers.
/// </summary>
public class PhoneNumbers
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual PhoneNumber Current { get; set; }

    /// <summary>
    /// Historic Phone Numbers.
    /// </summary>
    public virtual IEnumerable<PhoneNumber> HistoricPhoneNumbers { get; set; } = new List<PhoneNumber>();
}