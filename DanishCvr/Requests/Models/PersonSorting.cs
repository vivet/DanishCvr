using DanishCvr.Requests.Models.Enums;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Person Sorting.
/// </summary>
public class PersonSorting : BaseSorting
{
    /// <summary>
    /// By.
    /// </summary>
    public virtual PersonSortBy By { get; set; } = PersonSortBy.Relevance;
}