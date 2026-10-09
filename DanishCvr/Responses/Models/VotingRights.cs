using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Voting Rights.
/// </summary>
public class VotingRights
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Votes Current { get; set; }

    /// <summary>
    /// Historic Values.
    /// </summary>
    public virtual IEnumerable<Votes> HistoricValues { get; set; } = new List<Votes>();
}