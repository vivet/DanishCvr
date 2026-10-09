using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Types;

/// <summary>
/// Location.
/// </summary>
public class Within
{
    /// <summary>
    /// Location.
    /// </summary>
    public virtual Location Location { get; set; }

    /// <summary>
    /// Radius.
    /// In meters.
    /// </summary>
    [Range(1, int.MaxValue)]
    public virtual int Radius { get; set; } = 25000;
}