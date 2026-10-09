using DanishCvr.Requests.Models;

namespace DanishCvr.Requests;

/// <summary>
/// Base Search Request (abstract).
/// </summary>
public abstract class BaseSearchRequest<TCriteria, TPaging, TSorting>
    where TCriteria : class, new()
    where TPaging : class, new()
    where TSorting : BaseSorting, new()
{
    /// <summary>
    /// Criteria.
    /// </summary>
    public virtual TCriteria Criteria { get; set; } = new();

    /// <summary>
    /// Paging.
    /// </summary>
    public virtual TPaging Paging { get; set; } = new();

    /// <summary>
    /// Sorting.
    /// </summary>
    public virtual TSorting Sorting { get; set; } = new();
}