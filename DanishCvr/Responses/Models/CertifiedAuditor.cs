namespace DanishCvr.Responses.Models;

/// <summary>
/// Certified Auditor.
/// </summary>
public class CertifiedAuditor : BaseRelation
{
    /// <summary>
    /// Role.
    /// </summary>
    public virtual string Role { get; set; }

    /// <summary>
    /// Business Address.
    /// </summary>
    public virtual string BusinessAddress { get; set; }

    /// <summary>
    /// Voting Rights.
    /// </summary>
    public virtual AuditorVotingRights VotingRights { get; set; }
}