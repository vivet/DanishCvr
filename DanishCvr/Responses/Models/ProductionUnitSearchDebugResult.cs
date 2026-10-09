namespace DanishCvr.Responses.Models;

/// <summary>
/// Production Unit Search Result.
/// </summary>
public class ProductionUnitSearchDebugResult : BaseDebugResult
{
    /// <summary>
    /// Production Unit.
    /// </summary>
    public virtual ProductionUnitSearchResult ProductionUnit { get; set; }
}