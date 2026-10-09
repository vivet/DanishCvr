namespace DanishCvr.Responses.Models;

/// <summary>
/// Share.
/// </summary>
public class Share : DateAndPeriod
{
    /// <summary>
    /// Share Class.
    /// </summary>
    public virtual string ShareClass { get; set; }

    /// <summary>
    /// Share Percentage.
    /// </summary>
    public virtual double SharePercentage { get; set; }
}