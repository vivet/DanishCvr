namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit Id.
/// </summary>
public class ProductionUnitId : DateAndPeriod
{
    /// <summary>
    /// Production Unit Number.
    /// </summary>
    public virtual string ProductionUnitNumber { get; set; }
}