using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Paging.
/// </summary>
public class Paging
{
    /// <summary>
    /// Count.
    /// </summary>
    [Range(0, int.MaxValue)]
    public virtual int Count { get; set; } = 25;

    /// <summary>
    /// Skip.
    /// </summary>
    [Range(0, int.MaxValue)]
    public virtual int Skip { get; set; } = 0;
}