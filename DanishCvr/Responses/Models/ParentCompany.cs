namespace DanishCvr.Responses.Models;

/// <summary>
/// Parent Company.
/// </summary>
public class ParentCompany : DateAndPeriod
{
    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; }
}