namespace DanishCvr.Models;

/// <summary>
/// Virksomheds Status.
/// </summary>
public class VirksomhedsStatus : DatoOgPeriode
{
    /// <summary>
    /// Status.
    /// </summary>
    public virtual string Status { get; set; }
}