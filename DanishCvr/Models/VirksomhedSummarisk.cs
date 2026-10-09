using System;
using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Virksomhed Summarisk.
/// </summary>
public class VirksomhedSummarisk
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
    /// Fejl Registreret.
    /// </summary>
    public virtual bool FejlRegistreret { get; set; }

    /// <summary>
    /// sidst Indlaest
    /// </summary>
    public virtual DateTimeOffset? SidstIndlaest { get; set; }

    /// <summary>
    /// sidst Opdateret
    /// </summary>
    public virtual DateTimeOffset? SidstOpdateret { get; set; }

    /// <summary>
    /// Cvr Nummer.
    /// </summary>
    public virtual string CvrNummer { get; set; }

    /// <summary>
    /// Reg Nummer.
    /// </summary>
    public virtual IEnumerable<RegNummer> RegNummer { get; set; } = new List<RegNummer>();

    /// <summary>
    /// Navne.
    /// </summary>
    public virtual IEnumerable<Navne> Navne { get; set; } = new List<Navne>();

    /// <summary>
    /// Livs Forloeb.
    /// </summary>
    public virtual IEnumerable<LivsForloeb> LivsForloeb { get; set; } = new List<LivsForloeb>();

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
}