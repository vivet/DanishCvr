namespace DanishCvr.Models;

/// <summary>
/// Maaneds Beskaeftigelse.
/// </summary>
public class MaanedsBeskaeftigelse : Beskaeftigelse
{
    /// <summary>
    /// Maaned.
    /// </summary>
    public virtual int Maaned { get; set; }
}