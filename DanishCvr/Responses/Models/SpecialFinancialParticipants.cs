using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Special Financial Participants.
/// </summary>
public class SpecialFinancialParticipants
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<SpecialFinancialParticipant> Current { get; set; } = new List<SpecialFinancialParticipant>();

    /// <summary>
    /// Historic Special Financial Participants.
    /// </summary>
    public virtual IEnumerable<SpecialFinancialParticipant> HistoricSpecialFinancialParticipants { get; set; } = new List<SpecialFinancialParticipant>();
}