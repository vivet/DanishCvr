using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Deltager Person Metadata.
/// </summary>
public class DeltagerPersonMetadata
{
    /// <summary>
    /// Nyeste Beliggenheds Adresse.
    /// </summary>
    public virtual Adresse NyesteBeliggenhedsAdresse { get; set; }

    /// <summary>
    /// Nyeste Kontakt Oplysninger.
    /// </summary>
    public virtual List<string> NyesteKontaktOplysninger { get; set; }
}