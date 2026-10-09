using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Produktions Enhed Metadata.
/// </summary>
public class ProduktionsEnhedMetadata
{
    /// <summary>
    /// Nyeste Navn
    /// </summary>
    public virtual Navne NyesteNavn { get; set; }

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
    /// Nyeste Kontakt Oplysninger.
    /// </summary>
    public virtual List<string> NyesteKontaktOplysninger { get; set; }

    /// <summary>
    /// Nyeste Cvr Nummer Relation.
    /// </summary>
    public virtual string NyesteCvrNummerRelation { get; set; }

    /// <summary>
    /// Nyeste Aars Beskaeftigelse.
    /// </summary>
    public virtual AarsBeskaeftigelse NyesteAarsBeskaeftigelse { get; set; }

    /// <summary>
    /// Nyeste Kvartals Beskaeftigelse.
    /// </summary>
    public virtual KvartalsBeskaeftigelse NyesteKvartalsBeskaeftigelse { get; set; }

    /// <summary>
    /// Nyeste Erst Maaneds Beskaeftigelse.
    /// </summary>
    public virtual MaanedsBeskaeftigelse NyesteErstMaanedsBeskaeftigelse { get; set; }

    /// <summary>
    /// Sammensat Status.
    /// </summary>
    public virtual string SammensatStatus { get; set; }
}