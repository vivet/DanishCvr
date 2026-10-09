using System;
using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Virksomhed Metadata.
/// </summary>
public class VirksomhedMetadata
{
    /// <summary>
    /// Nyeste Navn
    /// </summary>
    public virtual Navne NyesteNavn { get; set; }

    /// <summary>
    /// Nyeste Bi Navne
    /// </summary>
    public virtual IEnumerable<string> NyesteBiNavne { get; set; }

    /// <summary>
    /// Nyeste Virksomheds Form.
    /// </summary>
    public virtual VirksomhedsForm NyesteVirksomhedsForm { get; set; }

    /// <summary>
    /// Nyeste Beliggenheds Adresse.
    /// </summary>
    public virtual Adresse NyesteBeliggenhedsAdresse { get; set; }

    /// <summary>
    /// Nyeste Hoved Branche.
    /// </summary>
    public virtual Branche NyesteHovedBranche { get; set; }

    /// <summary>
    /// Nyeste Bi Branche 1.
    /// </summary>
    public virtual Branche NyesteBiBranche1 { get; set; }

    /// <summary>
    /// Nyeste Bi Branche 2.
    /// </summary>
    public virtual Branche NyesteBiBranche2 { get; set; }

    /// <summary>
    /// Nyeste Bi Branche 3.
    /// </summary>
    public virtual Branche NyesteBiBranche3 { get; set; }

    /// <summary>
    /// Nyeste Status.
    /// </summary>
    public virtual Status NyesteStatus { get; set; }

    /// <summary>
    /// Nyeste Kontakt Oplysninger.
    /// </summary>
    public virtual List<string> NyesteKontaktOplysninger { get; set; }

    /// <summary>
    /// Antal P Enheder.
    /// </summary>
    public virtual int AntalPEnheder { get; set; }

    /// <summary>
    /// Nyeste Aars Beskaeftigelse.
    /// </summary>
    public virtual AarsBeskaeftigelse NyesteAarsBeskaeftigelse { get; set; }

    /// <summary>
    /// Nyeste Kvartals Beskaeftigelse.
    /// </summary>
    public virtual KvartalsBeskaeftigelse NyesteKvartalsBeskaeftigelse { get; set; }

    /// <summary>
    /// Nyeste Maaneds Beskaeftigelse.
    /// </summary>
    public virtual MaanedsBeskaeftigelse NyesteMaanedsBeskaeftigelse { get; set; }

    /// <summary>
    /// Nyeste Erst Maaneds Beskaeftigelse.
    /// </summary>
    public virtual MaanedsBeskaeftigelse NyesteErstMaanedsBeskaeftigelse { get; set; }

    /// <summary>
    /// Sammensat Status.
    /// </summary>
    public virtual string SammensatStatus { get; set; }

    /// <summary>
    /// Stiftelses Dato.
    /// </summary>
    public virtual DateOnly? StiftelsesDato { get; set; }

    /// <summary>
    /// Virknings Dato.
    /// </summary>
    public virtual DateOnly? VirkningsDato { get; set; }
}