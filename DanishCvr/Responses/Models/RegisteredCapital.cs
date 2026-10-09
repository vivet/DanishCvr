namespace DanishCvr.Responses.Models;

/// <summary>
/// Registered Capital.
/// </summary>
public class RegisteredCapital : DateAndPeriod
{
    /// <summary>
    /// Value.
    /// </summary>
    public virtual double? Value { get; set; }

    /// <summary>
    /// Currency.
    /// </summary>
    public virtual string Currency { get; set; }
}