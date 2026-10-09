namespace DanishCvr.Models;

/// <summary>
/// Status.
/// </summary>
public class Status : DatoOgPeriode
{
    /// <summary>
    /// Status Kode.
    /// For "statuskode" er udfaldsrummet:
    /// "1" - Dekret
    /// "2" - Ophævelse af dekret
    /// "3" - Regnskab og boafslutning
    /// "4" - Andre meddelelser
    /// "5" - Indkaldelse til fordringsprøvelse
    /// "6" - Skiftesamling
    /// "7" - Andre meddelelser
    /// "8" - Åbning af forhandling
    /// "9" - Stadfæstelse
    /// </summary>
    public virtual long StatusKode { get; set; }

    /// <summary>
    /// Kredit Oplysning Kode.
    /// For "kreditoplysningskode" er udfaldsrummet:
    /// "1" - Konkurs
    /// "3" - Tvangsakkord
    /// </summary>
    public virtual long KreditOplysningKode { get; set; }
}