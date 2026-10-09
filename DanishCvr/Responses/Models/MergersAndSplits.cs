namespace DanishCvr.Responses.Models;

/// <summary>
/// Mergers And Splits.
/// </summary>
public class MergersAndSplits : DateAndPeriod
{
    /// <summary>
    /// Externa lId.
    /// </summary>
    public virtual string ExternalId { get; set; }
}