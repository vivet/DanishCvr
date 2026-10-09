namespace DanishCvr.Models;

/// <summary>
/// Post Adresse.
/// </summary>
public class PostAdresse : DatoOgPeriode
{
    /// <summary>
    /// Co Navn.
    /// </summary>
    public virtual string CoNavn { get; set; }

    /// <summary>
    /// Fri Tekst.
    /// </summary>
    public virtual string FriTekst { get; set; }

    /// <summary>
    /// Lande Kode.
    /// </summary>
    public virtual string LandeKode { get; set; }
}