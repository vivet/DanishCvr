using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Types;

/// <summary>
/// Geo Coordinate.
/// </summary>
public class Location
{
    /// <summary>
    /// Latitude.
    /// </summary>
    [Range(-90, 90)]
    public virtual double Latitude { get; set; }

    /// <summary>
    /// Longitude.
    /// </summary>
    [Range(-180, 180)]
    public virtual double Longitude { get; set; }
}