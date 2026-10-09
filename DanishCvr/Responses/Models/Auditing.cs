namespace DanishCvr.Responses.Models;

/// <summary>
/// Auditing.
/// </summary>
public class Auditing
{
    /// <summary>
    /// Has Audit.
    /// </summary>
    public virtual bool? HasAudit { get; set; }

    /// <summary>
    /// Auditor.
    /// </summary>
    public virtual Auditors Auditor { get; set; }

    /// <summary>
    /// Sustainability Auditor.
    /// </summary>
    public virtual Auditors SustainabilityAuditor { get; set; }

    /// <summary>
    /// Is Overtaken By Financial Stability Authority.
    /// </summary>
    public virtual bool? IsOvertakenByFinancialStabilityAuthority { get; set; }
}