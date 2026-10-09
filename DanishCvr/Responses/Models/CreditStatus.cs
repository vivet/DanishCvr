namespace DanishCvr.Responses.Models;

/// <summary>
/// Credit Status.
/// </summary>
public class CreditStatus : DateAndPeriod
{
    /// <summary>
    /// Code.
    /// </summary>
    public virtual string Code { get; set; }

    /// <summary>
    /// Text.
    /// </summary>
    public virtual string Text { get; set; }

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }
}