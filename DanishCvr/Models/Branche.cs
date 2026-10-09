namespace DanishCvr.Models;

/// <summary>
/// Branche.
/// </summary>
public class Branche : DatoOgPeriode
{
    /// <summary>
    /// Branche Kode.
    /// </summary>
    public virtual string BrancheKode { get; set; }

    /// <summary>
    /// Branche Tekst
    /// </summary>
    public virtual string BrancheTekst { get; set; }
}