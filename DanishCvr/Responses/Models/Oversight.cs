namespace DanishCvr.Responses.Models;

/// <summary>
/// Oversights.
/// </summary>
public class Oversight
{
    /// <summary>
    /// Authority.
    /// </summary>
    public virtual string Authority { get; set; }

    /// <summary>
    /// Has Social Economic Oversight.
    /// </summary>
    public virtual bool? HasSocialEconomicOversight { get; set; }

    /// <summary>
    /// Has Special Licenses Or Concessions.
    /// </summary>
    public virtual bool? HasSpecialLicensesOrConcessions { get; set; }

    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }
}