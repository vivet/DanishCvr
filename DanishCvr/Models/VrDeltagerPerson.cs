using System;
using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Vr Deltager Person.
/// </summary>
public class VrDeltagerPerson : VrDeltager
{
    /// <summary>
    /// Stilling.
    /// </summary>
    public virtual string Stilling { get; set; }

    /// <summary>
    /// Status Kode.
    /// </summary>
    public virtual string StatusKode { get; set; }

    /// <summary>
    /// Deltager Person Metadata.
    /// </summary>
    public virtual DeltagerPersonMetadata DeltagerPersonMetadata { get; set; }

    /// <summary>
    /// Samt Id
    /// </summary>
    public virtual long SamtId { get; set; }

    /// <summary>
    /// Fejl Registreret.
    /// </summary>
    public virtual bool FejlRegistreret { get; set; }

    /// <summary>
    /// Data Adgang.
    /// </summary>
    public virtual int DataAdgang { get; set; }

    /// <summary>
    /// Fejl Ved Indlaesning.
    /// </summary>
    public virtual bool FejlVedIndlaesning { get; set; }

    /// <summary>
    /// naermeste Fremtidige Dato.
    /// </summary>
    public virtual DateOnly? NaermesteFremtidigeDato { get; set; }

    /// <summary>
    /// Fejl Beskrivelse.
    /// </summary>
    public virtual string FejlBeskrivelse { get; set; }

    /// <summary>
    /// Virknings Aktoer.
    /// </summary>
    public virtual string VirkningsAktoer { get; set; }

    /// <summary>
    /// Telefon Nummer.
    /// </summary>
    public virtual IEnumerable<Kontakt> TelefonNummer { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Telefax Nummer.
    /// </summary>
    public virtual IEnumerable<Kontakt> TelefaxNummer { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Elektronisk Post
    /// </summary>
    public virtual IEnumerable<Kontakt> ElektroniskPost { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Virksomhed Summarisk Relation.
    /// </summary>
    public virtual IEnumerable<VirksomhedSummariskRelation> VirksomhedSummariskRelation { get; set; } = new List<VirksomhedSummariskRelation>();
}