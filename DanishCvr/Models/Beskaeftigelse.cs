using System;

namespace DanishCvr.Models;

/// <summary>
/// Beskaeftigelse.
/// </summary>
public class Beskaeftigelse
{
    /// <summary>
    /// Aar.
    /// </summary>
    public virtual int Aar { get; set; }

    /// <summary>
    /// Antal Aarsvaerk.
    /// </summary>
    public virtual double AntalAarsvaerk { get; set; }

    /// <summary>
    /// Antal Ansatte.
    /// </summary>
    public virtual int? AntalAnsatte { get; set; }

    /// <summary>
    /// Sidst Opdateret.
    /// </summary>
    public virtual DateTimeOffset? SidstOpdateret { get; set; }

    /// <summary>
    /// Interval Kode Antal Aarsvaerk.
    /// </summary>
    public virtual string IntervalKodeAntalAarsvaerk { get; set; }

    /// <summary>
    /// Interval Kode Antal Ansatte.
    /// </summary>
    public virtual string IntervalKodeAntalAnsatte { get; set; }
}