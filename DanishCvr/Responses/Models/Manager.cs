namespace DanishCvr.Responses.Models;

/// <summary>
/// Manager.
/// </summary>
public class Manager : BaseRelation
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