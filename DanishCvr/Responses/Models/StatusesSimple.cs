namespace DanishCvr.Responses.Models;

/// <summary>
/// Statuses Simple.
/// </summary>
public class StatusesSimple
{
    /// <summary>
    /// Current.
    /// </summary>
    public virtual Status Current { get; set; }

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool IsActive { get; set; }
}