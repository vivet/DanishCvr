using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Liable Participants.
/// </summary>
public class RelationLiableParticipants
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationLiableParticipant Current { get; set; }

    /// <summary>
    /// Historic Liable Participants.
    /// </summary>
    public virtual IEnumerable<RelationLiableParticipant> HistoricLiableParticipants { get; set; } = new List<RelationLiableParticipant>();
}