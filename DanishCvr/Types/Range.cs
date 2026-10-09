using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Types;

/// <summary>
/// Range.
/// </summary>
public class Range
{
    /// <summary>
    /// From.
    /// </summary>
    [Range(0, int.MaxValue)]
    public virtual int? From { get; set; }

    /// <summary>
    /// To.
    /// </summary>
    [Range(0, int.MaxValue)]
    public virtual int? To { get; set; }
}