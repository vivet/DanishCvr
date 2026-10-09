using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Special Financial Participants.
/// </summary>
public class RelationSpecialFinancialParticipants
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationSpecialFinancialParticipant Current { get; set; }

    /// <summary>
    /// Historic Special Financial Participants.
    /// </summary>
    public virtual IEnumerable<RelationSpecialFinancialParticipant> HistoricSpecialFinancialParticipants { get; set; } = new List<RelationSpecialFinancialParticipant>();
}