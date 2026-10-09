using System;
using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Vr Virksomhed.
/// </summary>
public class VrVirksomhed
{
    /// <summary>
    /// Cvr Nummer.
    /// </summary>
    public virtual string CvrNummer { get; set; }

    /// <summary>
    /// Reg Nummer.
    /// </summary>
    public virtual IEnumerable<RegNummer> RegNummer { get; set; } = new List<RegNummer>();

    /// <summary>
    /// Branche Ansvarskode.
    /// </summary>
    public virtual long? BrancheAnsvarskode { get; set; }

    /// <summary>
    /// Reklame Beskyttet.
    /// </summary>
    public virtual bool ReklameBeskyttet { get; set; }

    /// <summary>
    /// Navne.
    /// </summary>
    public virtual IEnumerable<Navne> Navne { get; set; } = new List<Navne>();

    /// <summary>
    /// Bi Navne.
    /// </summary>
    public virtual IEnumerable<Navne> BiNavne { get; set; } = new List<Navne>();

    /// <summary>
    /// Post Adresse.
    /// </summary>
    public virtual IEnumerable<PostAdresse> PostAdresse { get; set; } = new List<PostAdresse>();

    /// <summary>
    /// Beliggenheds Adresse.
    /// </summary>
    public virtual IEnumerable<Adresse> BeliggenhedsAdresse { get; set; } = new List<Adresse>();

    /// <summary>
    /// Telefon Nummer.
    /// </summary>
    public virtual IEnumerable<Kontakt> TelefonNummer { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Telefax Nummer.
    /// </summary>
    public virtual IEnumerable<Kontakt> TelefaxNummer { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Sekundaert Telefon Nummer.
    /// </summary>
    public virtual IEnumerable<Kontakt> SekundaertTelefonNummer { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Sekundaert Telefax Nummer.
    /// </summary>
    public virtual IEnumerable<Kontakt> SekundaertTelefaxNummer { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Elektronisk Post
    /// </summary>
    public virtual IEnumerable<Kontakt> ElektroniskPost { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Hjemmeside.
    /// </summary>
    public virtual IEnumerable<Kontakt> Hjemmeside { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Obligatorisk Email.
    /// </summary>
    public virtual IEnumerable<Kontakt> ObligatoriskEmail { get; set; } = new List<Kontakt>();

    /// <summary>
    /// Livs Forloeb.
    /// </summary>
    public virtual IEnumerable<LivsForloeb> LivsForloeb { get; set; } = new List<LivsForloeb>();

    /// <summary>
    /// Hoved branche.
    /// </summary>
    public virtual IEnumerable<Branche> HovedBranche { get; set; } = new List<Branche>();

    /// <summary>
    /// Bi Branche 1.
    /// </summary>
    public virtual IEnumerable<Branche> BiBranche1 { get; set; } = new List<Branche>();

    /// <summary>
    /// Bi Branche 2.
    /// </summary>
    public virtual IEnumerable<Branche> BiBranche2 { get; set; } = new List<Branche>();

    /// <summary>
    /// Bi Branche 3.
    /// </summary>
    public virtual IEnumerable<Branche> BiBranche3 { get; set; } = new List<Branche>();

    /// <summary>
    /// Status.
    /// </summary>
    public virtual IEnumerable<Status> Status { get; set; } = new List<Status>();

    /// <summary>
    /// Virksomheds Status.
    /// </summary>
    public virtual IEnumerable<VirksomhedsStatus> VirksomhedsStatus { get; set; } = new List<VirksomhedsStatus>();

    /// <summary>
    /// Virksomheds Form.
    /// </summary>
    public virtual IEnumerable<VirksomhedsForm> VirksomhedsForm { get; set; } = new List<VirksomhedsForm>();

    /// <summary>
    /// Aars Beskaeftigelse.
    /// </summary>
    public virtual IEnumerable<AarsBeskaeftigelse> AarsBeskaeftigelse { get; set; } = new List<AarsBeskaeftigelse>();

    /// <summary>
    /// Kvartals Beskaeftigelse.
    /// </summary>
    public virtual IEnumerable<KvartalsBeskaeftigelse> KvartalsBeskaeftigelse { get; set; } = new List<KvartalsBeskaeftigelse>();

    /// <summary>
    /// Maaneds Beskaeftigelse.
    /// </summary>
    public virtual IEnumerable<MaanedsBeskaeftigelse> MaanedsBeskaeftigelse { get; set; } = new List<MaanedsBeskaeftigelse>();

    /// <summary>
    /// Erst Maaneds Beskaeftigelse.
    /// </summary>
    public virtual IEnumerable<MaanedsBeskaeftigelse> ErstMaanedsBeskaeftigelse { get; set; } = new List<MaanedsBeskaeftigelse>();

    /// <summary>
    /// Attributter.
    /// </summary>
    public virtual IEnumerable<Attributter> Attributter { get; set; } = new List<Attributter>();

    /// <summary>
    /// P Enheder.
    /// </summary>
    public virtual IEnumerable<PEnhed> PEnheder { get; set; } = new List<PEnhed>();

    /// <summary>
    /// Deltager Relation
    /// </summary>
    public virtual IEnumerable<DeltagerRelation> DeltagerRelation { get; set; } = new List<DeltagerRelation>();

    /// <summary>
    /// Fusioner.
    /// </summary>
    public virtual IEnumerable<FusionSpaltning> Fusioner { get; set; } = new List<FusionSpaltning>();

    /// <summary>
    /// Spaltninger.
    /// </summary>
    public virtual IEnumerable<FusionSpaltning> Spaltninger { get; set; } = new List<FusionSpaltning>();

    /// <summary>
    /// Virksomhed Metadata.
    /// </summary>
    public virtual VirksomhedMetadata VirksomhedMetadata { get; set; }

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
    /// Enheds Nummer.
    /// </summary>
    public virtual string EnhedsNummer { get; set; }

    /// <summary>
    /// Enheds Type
    /// </summary>
    public virtual string EnhedsType { get; set; }

    /// <summary>
    /// sidst Indlaest
    /// </summary>
    public virtual DateTimeOffset? SidstIndlaest { get; set; }

    /// <summary>
    /// sidst Opdateret
    /// </summary>
    public virtual DateTimeOffset? SidstOpdateret { get; set; }

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
}