using System;

namespace DanishCvr.Models;

/// <summary>
/// Adresse.
/// </summary>
public class Adresse : DatoOgPeriode
{
    /// <summary>
    /// Lande Kode.
    /// </summary>
    public virtual string LandeKode { get; set; }

    /// <summary>
    /// Fri Tekst.
    /// </summary>
    public virtual string FriTekst { get; set; }

    /// <summary>
    /// Vej Kode.
    /// </summary>
    public virtual int VejKode { get; set; }

    /// <summary>
    /// Kommune.
    /// </summary>
    public virtual Kommune Kommune { get; set; } = new();

    /// <summary>
    /// Husnummer Fra.
    /// </summary>
    public virtual string HusnummerFra { get; set; }

    /// <summary>
    /// Adresse Id.
    /// </summary>
    public virtual string AdresseId { get; set; }

    /// <summary>
    /// Sidst Valideret.
    /// </summary>
    public virtual DateTimeOffset? SidstValideret { get; set; }

    /// <summary>
    /// Husnummer Til.
    /// </summary>
    public virtual string HusnummerTil { get; set; }

    /// <summary>
    /// Bogstav Fra.
    /// </summary>
    public virtual string BogstavFra { get; set; }

    /// <summary>
    /// Bogstav Til.
    /// </summary>
    public virtual string BogstavTil { get; set; }

    /// <summary>
    /// Etage.
    /// </summary>
    public virtual string Etage { get; set; }

    /// <summary>
    /// Side Doer.
    /// </summary>
    public virtual string SideDoer { get; set; }

    /// <summary>
    /// Co Navn.
    /// </summary>
    public virtual string CoNavn { get; set; }

    /// <summary>
    /// Postboks.
    /// </summary>
    public virtual string Postboks { get; set; }

    /// <summary>
    /// Vej Navn.
    /// </summary>
    public virtual string VejNavn { get; set; }

    /// <summary>
    /// By Navn.
    /// </summary>
    public virtual string ByNavn { get; set; }

    /// <summary>
    /// Post Nummer.
    /// </summary>
    public virtual string PostNummer { get; set; }

    /// <summary>
    /// Post Distrikt.
    /// </summary>
    public virtual string PostDistrikt { get; set; }
}