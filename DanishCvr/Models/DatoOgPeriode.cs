using System;

namespace DanishCvr.Models;

/// <summary>
/// Dato Og Periode.
/// </summary>
public class DatoOgPeriode
{
    /// <summary>
    /// Periode.
    /// </summary>
    public virtual Periode Periode { get; set; } = new();

    /// <summary>
    /// Sidst Opdateret.
    /// </summary>
    public virtual DateTimeOffset? SidstOpdateret { get; set; }
}