namespace DanishCvr.Responses.Models;

/// <summary>
/// Business Type.
/// </summary>
public class BusinessType : DateAndPeriod
{
    /// <summary>
    /// Code.
    /// </summary>
    public virtual string Code { get; set; }

    /// <summary>
    /// Abbreviation.
    /// </summary>
    public virtual string Abbreviation { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public virtual string Description { get; set; }
}