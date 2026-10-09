namespace DanishCvr.Models;

/// <summary>
/// Kommune.
/// </summary>
public class Kommune : DatoOgPeriode
{
    /// <summary>
    /// Kommune Kode.
    /// </summary>
    public virtual int KommuneKode { get; set; }

    /// <summary>
    /// Kommune Navn.
    /// </summary>
    public virtual string KommuneNavn { get; set; }
}