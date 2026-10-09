namespace DanishCvr.Responses.Models;

/// <summary>
/// Liquidator.
/// </summary>
public class Liquidator : BaseRelation
{
    /// <summary>
    /// Title.
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// Appointed By.
    /// </summary>
    public virtual string AppointedBy { get; set; }
}