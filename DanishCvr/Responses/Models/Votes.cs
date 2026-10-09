namespace DanishCvr.Responses.Models;

/// <summary>
/// Voting Rights.
/// </summary>
public class Votes : DateAndPeriod
{
    /// <summary>
    /// Value.
    /// </summary>
    public virtual double Value { get; set; }
}