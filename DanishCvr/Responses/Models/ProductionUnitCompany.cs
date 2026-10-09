namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit Company.
/// </summary>
public class ProductionUnitCompany : DateAndPeriod
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }
}