namespace DanishCvr.Responses.Models;

/// <summary>
/// Financial Year.
/// </summary>
public class FinancialYear : DateAndPeriod
{
    /// <summary>
    /// Begin.
    /// </summary>
    public virtual MonthAndDay Begin { get; set; }

    /// <summary>
    /// End.
    /// </summary>
    public virtual MonthAndDay End { get; set; }
}