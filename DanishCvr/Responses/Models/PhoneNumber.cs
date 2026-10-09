namespace DanishCvr.Responses.Models;

/// <summary>
/// Phone Number.
/// </summary>
public class PhoneNumber : DateAndPeriod
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