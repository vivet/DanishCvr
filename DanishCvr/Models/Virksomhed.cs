namespace DanishCvr.Models;

/// <summary>
/// Virksomhed.
/// </summary>
public class Virksomhed
{
    /// <summary>
    /// Vr Virksomhed.
    /// </summary>
    public virtual VrVirksomhed VrVirksomhed { get; set; } = new();
}