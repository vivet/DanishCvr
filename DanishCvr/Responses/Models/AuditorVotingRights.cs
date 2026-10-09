namespace DanishCvr.Responses.Models;

/// <summary>
/// Auditor Voting Rights.
/// </summary>
public class AuditorVotingRights : VotingRights
{
    /// <summary>
    /// Type.
    /// </summary>
    public virtual string Type { get; set; }

    /// <summary>
    /// Exception.
    /// </summary>
    public virtual string Exception { get; set; }
}