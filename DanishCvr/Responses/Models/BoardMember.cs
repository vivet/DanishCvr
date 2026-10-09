namespace DanishCvr.Responses.Models;

/// <summary>
/// Board Member.
/// </summary>
public class BoardMember : BaseRelation
{
    /// <summary>
    /// Title.
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// Election Method.
    /// </summary>
    public virtual string ElectionMethod { get; set; }

    /// <summary>
    /// Alternate For.
    /// </summary>
    public virtual string AlternateFor { get; set; }

    /// <summary>
    /// Is Directive 8 Approved.
    /// </summary>
    public virtual bool? IsDirective8Approved { get; set; }
}