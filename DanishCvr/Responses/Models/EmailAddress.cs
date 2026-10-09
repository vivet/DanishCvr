namespace DanishCvr.Responses.Models;

/// <summary>
/// Email Address.
/// </summary>
public class EmailAddress : DateAndPeriod
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