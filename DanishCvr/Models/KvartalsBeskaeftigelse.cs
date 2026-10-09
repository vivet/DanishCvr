namespace DanishCvr.Models;

/// <summary>
/// Kvartals Beskaeftigelse.
/// </summary>
public class KvartalsBeskaeftigelse : Beskaeftigelse
{
    /// <summary>
    /// Kvartal.
    /// </summary>
    public virtual int Kvartal { get; set; }
}