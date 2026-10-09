namespace DanishCvr.Responses.Models;

/// <summary>
/// Beneficial Owner.
/// </summary>
public class BeneficialOwner : BaseRelation
{
    /// <summary>
    /// Equity.
    /// </summary>
    public virtual Equity Equity { get; set; } = new();

    /// <summary>
    /// Ownership Special.
    /// </summary>
    public virtual OwnershipSpecials OwnershipSpecial { get; set; } = new();

    /// <summary>
    /// Voting Rights.
    /// </summary>
    public virtual VotingRights VotingRights { get; set; } = new();

    /// <summary>
    /// Collateral Voting Rights.
    /// </summary>
    public virtual VotingRights CollateralVotingRights { get; set; } = new();

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }
}