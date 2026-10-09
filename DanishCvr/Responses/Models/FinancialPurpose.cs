namespace DanishCvr.Responses.Models;

/// <summary>
/// Financial Purpose.
/// </summary>
public class FinancialPurpose
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Purpose Current { get; set; }

    /// <summary>
    /// Latest.
    /// </summary>
    public virtual Purpose Latest { get; set; }
}