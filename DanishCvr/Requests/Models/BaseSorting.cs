using DanishCvr.Requests.Models.Enums;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Base Sorting (abstract)
/// </summary>
public abstract class BaseSorting
{
    /// <summary>
    /// Direction.
    /// </summary>
    public virtual SortDirection Direction { get; set; } = SortDirection.Ascending;
}