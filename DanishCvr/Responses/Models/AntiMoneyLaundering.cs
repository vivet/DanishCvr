using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Anti Money Laundering
/// </summary>
public class AntiMoneyLaundering
{
    /// <summary>
    /// Text.
    /// </summary>
    public virtual string Text { get; set; }

    /// <summary>
    /// Activities.
    /// </summary>
    public virtual IEnumerable<string> Activities { get; set; } = new List<string>();

    /// <summary>
    /// Is Subjected To Law About Money Laundering And Terror Financing.
    /// </summary>
    public virtual bool? IsSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing { get; set; }

    /// <summary>
    /// Anti Money Laundering Officer.
    /// </summary>
    public virtual AntiMoneyLaunderingAppointees AntiMoneyLaunderingAppointees { get; set; }
}