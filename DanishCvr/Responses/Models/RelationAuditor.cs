namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Auditor.
/// </summary>
public class RelationAuditor : BaseRelationRole
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }
}