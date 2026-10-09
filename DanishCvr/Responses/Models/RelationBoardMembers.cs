using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Board Members.
/// </summary>
public class RelationBoardMembers
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual RelationBoardMember Current { get; set; }

    /// <summary>
    /// Historic Board Members.
    /// </summary>
    public virtual IEnumerable<RelationBoardMember> HistoricBoardMembers { get; set; } = new List<RelationBoardMember>();
}