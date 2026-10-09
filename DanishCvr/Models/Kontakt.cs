namespace DanishCvr.Models;

/// <summary>
/// Kontakt.
/// </summary>
public class Kontakt : DatoOgPeriode
{
    /// <summary>
    /// Kontakt Oplysning.
    /// </summary>
    public virtual string KontaktOplysning { get; set; }

    /// <summary>
    /// Hemmelig.
    /// </summary>
    public virtual bool Hemmelig { get; set; }
}