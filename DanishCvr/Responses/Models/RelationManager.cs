namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Manager.
/// </summary>
public class RelationManager : BaseRelationRole
{
    /// <summary>
    /// Title.
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// Election Method.
    /// </summary>
    public virtual string ElectionMethod { get; set; }
}