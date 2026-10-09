using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Liable Participants.
/// </summary>
public class LiableParticipants
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<LiableParticipant> Current { get; set; } = new List<LiableParticipant>();

    /// <summary>
    /// Historic Liable Participants.
    /// </summary>
    public virtual IEnumerable<LiableParticipant> HistoricLiableParticipants { get; set; } = new List<LiableParticipant>();
}