namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Legal Owner.
/// </summary>
public class RelationLegalOwner : BaseRelationRole
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }

    /// <summary>
    /// Equity.
    /// </summary>
    public virtual Equity Equity { get; set; } = new();

    /// <summary>
    /// Voting Rights.
    /// </summary>
    public virtual VotingRights VotingRights { get; set; } = new();

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }
}