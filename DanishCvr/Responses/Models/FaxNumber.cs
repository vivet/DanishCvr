namespace DanishCvr.Responses.Models;

/// <summary>
/// Fax Number
/// </summary>
public class FaxNumber : DateAndPeriod
{
    /// <summary>
    /// Value.
    /// </summary>
    public virtual string Value { get; set; }

    /// <summary>
    /// Is Unlisted.
    /// </summary>
    public virtual bool IsUnlisted { get; set; } = false;
}