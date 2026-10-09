using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Authority.
/// </summary>
public class Authority
{
    /// <summary>
    /// Signatory Rule.
    /// </summary>
    public virtual string SignatoryRule { get; set; }

    /// <summary>
    /// Executives.
    /// </summary>
    public virtual Executives Executives { get; set; }

    /// <summary>
    /// Authorized Signatories.
    /// </summary>
    public virtual AuthorizedSignatories AuthorizedSignatories { get; set; }

    /// <summary>
    /// Special Financial Participants.
    /// </summary>
    public virtual SpecialFinancialParticipants SpecialFinancialParticipants { get; set; }

    /// <summary>
    /// Other Liable Participants.
    /// </summary>
    public virtual LiableParticipants OtherLiableParticipants { get; set; }

    /// <summary>
    /// Liquidators.
    /// </summary>
    public virtual IEnumerable<Liquidator> Liquidators { get; set; } = new List<Liquidator>();
}