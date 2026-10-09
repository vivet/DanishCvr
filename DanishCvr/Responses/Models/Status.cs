namespace DanishCvr.Responses.Models;

/// <summary>
/// Status.
/// </summary>
public class Status : DateAndPeriod
{
    /// <summary>
    /// Value.
    /// </summary>
    public virtual string Value { get; set; }
}