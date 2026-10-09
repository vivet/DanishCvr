namespace DanishCvr.Responses.Models;

/// <summary>
/// Industry.
/// </summary>
public class Industry : DateAndPeriod
{
    /// <summary>
    /// Code.
    /// </summary>
    public virtual string Code { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public virtual string Description { get; set; }
}