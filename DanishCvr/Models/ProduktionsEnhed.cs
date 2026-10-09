namespace DanishCvr.Models;

/// <summary>
/// Produktions Enhed.
/// </summary>
public class ProduktionsEnhed
{
    /// <summary>
    /// Vr Produktions Enhed.
    /// </summary>
    public virtual VrProduktionsEnhed VrProduktionsEnhed { get; set; } = new();
}