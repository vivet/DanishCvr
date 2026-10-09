using System;

namespace DanishCvr.Models;

/// <summary>
/// Periode.
/// </summary>
public class Periode
{
    /// <summary>
    /// Gyldig Fra.
    /// </summary>
    public virtual DateOnly? GyldigFra { get; set; }

    /// <summary>
    /// Gyldig Til.
    /// </summary>
    public virtual DateOnly? GyldigTil { get; set; }

    /// <summary>
    /// Is Valid.
    /// </summary>
    /// <returns>Whether the <see cref="Periode"/> is valid.</returns>
    public virtual bool IsActive()
    {
        return
            (
                this.GyldigFra == null ||
                this.GyldigFra <= DateOnly.FromDateTime(DateTime.UtcNow)
            ) &&
            (
                this.GyldigTil == null ||
                this.GyldigTil >= DateOnly.FromDateTime(DateTime.UtcNow)
            );
    }
}