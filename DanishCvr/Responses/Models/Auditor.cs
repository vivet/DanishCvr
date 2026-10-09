namespace DanishCvr.Responses.Models;

/// <summary>
/// Auditor.
/// </summary>
public class Auditor : BaseRelation
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }
}