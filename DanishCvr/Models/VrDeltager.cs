using System;
using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Vr Deltager.
/// </summary>
public class VrDeltager
{
    /// <summary>
    /// Enheds Nummer.
    /// </summary>
    public virtual string EnhedsNummer { get; set; }

    /// <summary>
    /// Enheds Type
    /// </summary>
    public virtual string EnhedsType { get; set; }

    /// <summary>
    /// Forretnings Noegle.
    /// </summary>
    public virtual string ForretningsNoegle { get; set; } 

    /// <summary>
    /// Organisations Type.
    /// </summary>
    public virtual string OrganisationsType { get; set; }

    /// <summary>
    /// Sidst Indlaest
    /// </summary>
    public virtual DateTimeOffset? SidstIndlaest { get; set; }

    /// <summary>
    /// Sidst Opdateret.
    /// </summary>
    public virtual DateTimeOffset? SidstOpdateret { get; set; }

    /// <summary>
    /// Navne.
    /// </summary>
    public virtual IEnumerable<Navne> Navne { get; set; } = new List<Navne>();

    /// <summary>
    /// Adresse Hemmelig.
    /// </summary>
    public virtual bool AdresseHemmelig { get; set; }

    /// <summary>
    /// Adresse Hemmelig Undtagelse.
    /// </summary>
    public virtual bool AdresseHemmeligUndtagelse { get; set; }

    /// <summary>
    /// Adresse Opdatering Ophoert.
    /// </summary>
    public virtual bool AdresseOpdateringOphoert { get; set; }

    /// <summary>
    /// Beliggenheds Adresse.
    /// </summary>
    public virtual IEnumerable<Adresse> BeliggenhedsAdresse { get; set; } = new List<Adresse>();

    /// <summary>
    /// Post Adresse.
    /// </summary>
    public virtual IEnumerable<PostAdresse> PostAdresse { get; set; } = new List<PostAdresse>();
}