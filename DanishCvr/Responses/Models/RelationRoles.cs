namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Roles.
/// </summary>
public class RelationRoles
{
    /// <summary>
    /// Founder.
    /// </summary>
    public virtual RelationFounder Founder { get; set; }

    /// <summary>
    /// Legal Owner.
    /// </summary>
    public virtual RelationLegalOwner LegalOwner { get; set; }

    /// <summary>
    /// Beneficial Owner.
    /// </summary>
    public virtual RelationBeneficialOwner BeneficialOwner { get; set; }

    /// <summary>
    /// Liquidator.
    /// </summary>
    public virtual RelationLiquidator Liquidator { get; set; }

    /// <summary>
    /// Auditors.
    /// </summary>
    public virtual RelationAuditors Auditors { get; set; }

    /// <summary>
    /// Executives.
    /// </summary>
    public virtual RelationExecutives Executives { get; set; }

    /// <summary>
    /// Board Members.
    /// </summary>
    public virtual RelationBoardMembers BoardMembers { get; set; }

    /// <summary>
    /// Managers.
    /// </summary>
    public virtual RelationManagers Managers { get; set; }

    /// <summary>
    /// Anti Money Laundering Appointees.
    /// </summary>
    public virtual RelationAntiMoneyLaunderingAppointees AntiMoneyLaunderingAppointees { get; set; }

    /// <summary>
    /// Authorized Signatories.
    /// </summary>
    public virtual RelationAuthorizedSignatories AuthorizedSignatories { get; set; }

    /// <summary>
    /// Special Financial Participants.
    /// </summary>
    public virtual RelationSpecialFinancialParticipants SpecialFinancialParticipants { get; set; }

    /// <summary>
    /// Liable Participants.
    /// </summary>
    public virtual RelationLiableParticipants LiableParticipants { get; set; }

    /// <summary>
    /// Association Representatives.
    /// </summary>
    public virtual RelationAssociationRepresentatives AssociationRepresentatives { get; set; }

    /// <summary>
    /// Certified Auditors.
    /// </summary>
    public virtual RelationCertifiedAuditors CertifiedAuditors { get; set; }
}