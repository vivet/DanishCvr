using System;

namespace DanishCvr.Types;

/// <summary>
/// Period.
/// </summary>
public class Period
{
    /// <summary>
    /// From.
    /// </summary>
    public virtual DateOnly? From { get; set; }

    /// <summary>
    /// To.
    /// </summary>
    public virtual DateOnly? To { get; set; }

    /// <summary>
    /// Is Valid.
    /// </summary>
    /// <returns>Whether the <see cref="Period"/> is valid.</returns>
    public virtual bool IsValid()
    {
        return
            (
                this.From == null ||
                this.From <= DateOnly.FromDateTime(DateTime.UtcNow)
            ) &&
            (
                this.To == null ||
                this.To >= DateOnly.FromDateTime(DateTime.UtcNow)
            );
    }
}