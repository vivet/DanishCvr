using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Board Members.
/// </summary>
public class BoardMembers
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual IEnumerable<BoardMember> Current { get; set; } = new List<BoardMember>();

    /// <summary>
    /// Historic Board Members.
    /// </summary>
    public virtual IEnumerable<BoardMember> HistoricBoardMembers { get; set; } = new List<BoardMember>();
}