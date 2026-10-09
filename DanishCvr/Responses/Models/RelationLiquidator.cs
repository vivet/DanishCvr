namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Liquidator.
/// </summary>
public class RelationLiquidator : BaseRelationRole
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