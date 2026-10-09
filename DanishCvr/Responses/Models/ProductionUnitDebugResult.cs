namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit Result.
/// </summary>
public class ProductionUnitDebugResult : BaseDebugResult
{
    /// <summary>
    /// Production Unit.
    /// </summary>
    public virtual ProductionUnitResult ProductionUnit { get; set; }
}