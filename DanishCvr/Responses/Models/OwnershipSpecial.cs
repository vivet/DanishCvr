namespace DanishCvr.Responses.Models;

/// <summary>
/// Ownership Special.
/// </summary>
public class OwnershipSpecial : DateAndPeriod
{
    /// <summary>
    /// Value.
    /// </summary>
    public virtual string Value { get; set; }

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }
}