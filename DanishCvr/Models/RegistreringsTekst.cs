using System.Collections.Generic;
using System;

namespace DanishCvr.Models;

/// <summary>
/// Registrerings Tekst.
/// </summary>
public class RegistreringsTekst
{
    /// <summary>
    /// Offentliggoerelse Id.
    /// </summary>
    public virtual string OffentliggoerelseId { get; set; }

    /// <summary>
    /// Registrering Tidsstempel.
    /// </summary>
    public virtual DateTimeOffset? RegistreringTidsstempel { get; set; }

    /// <summary>
    /// Offentliggoerelse Tidsstempel.
    /// </summary>
    public virtual DateTimeOffset? OffentliggoerelseTidsstempel { get; set; }

    /// <summary>
    /// Cvr Nummer.
    /// </summary>
    public virtual string CvrNummer { get; set; }

    /// <summary>
    /// Hoved Navn.
    /// </summary>
    public virtual string HovedNavn { get; set; }

    /// <summary>
    /// Adresse.
    /// </summary>
    public virtual string Adresse { get; set; }

    /// <summary>
    /// Kommune Kode.
    /// </summary>
    public virtual string KommuneKode { get; set; }

    /// <summary>
    /// Postnummer.
    /// </summary>
    public virtual string Postnummer { get; set; }

    /// <summary>
    /// Virksomheds Formkode.
    /// </summary>
    public virtual string VirksomhedsFormkode { get; set; }

    /// <summary>
    /// Tekst.
    /// </summary>
    public virtual string Tekst { get; set; }

    /// <summary>
    /// Virksomheds Registrering Statusser.
    /// </summary>
    public virtual IEnumerable<string> VirksomhedsRegistreringStatusser { get; set; } = new List<string>();

    /// <summary>
    /// Oprettet.
    /// </summary>
    public virtual DateTimeOffset? Oprettet { get; set; }

    /// <summary>
    /// Opdateret.
    /// </summary>
    public virtual DateTimeOffset? Opdateret { get; set; }

    /// <summary>
    /// Sidst Opdateret.
    /// </summary>
    public virtual DateTimeOffset? SidstOpdateret { get; set; }
}