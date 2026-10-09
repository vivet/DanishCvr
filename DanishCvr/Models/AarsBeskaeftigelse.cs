namespace DanishCvr.Models;

/// <summary>
/// Aars Beskaeftigelse.
/// </summary>
public class AarsBeskaeftigelse : Beskaeftigelse
{
    /// <summary>
    /// Antal Inklusiv Ejere.
    /// </summary>
    public virtual int? AntalInklusivEjere { get; set; }

    /// <summary>
    /// Interval Kode Antal Inklusiv Ejere.
    /// </summary>
    public virtual string IntervalKodeAntalInklusivEjere { get; set; }
}