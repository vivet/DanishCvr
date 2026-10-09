namespace DanishCvr.Responses.Models;

/// <summary>
/// Auditor Registration.
/// </summary>
public class AuditorRegistration
{
    /// <summary>
    /// Type.
    /// </summary>
    public virtual string Type { get; set; }

    /// <summary>
    /// Is Holding Company.
    /// </summary>
    public virtual bool? IsHoldingCompany { get; set; }

    /// <summary>
    /// Public Interest.
    /// </summary>
    public virtual string PublicInterest { get; set; }

    /// <summary>
    /// Contact Person.
    /// </summary>
    public virtual string ContactPerson { get; set; }

    /// <summary>
    /// Professional Network.
    /// </summary>
    public virtual string ProfessionalNetwork { get; set; }

    /// <summary>
    /// Certified Auditors.
    /// </summary>
    public virtual CertifiedAuditors CertifiedAuditors { get; set; }
}