using DanishCvr.Requests.Models.Enums;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Company Sorting.
/// </summary>
public class CompanySorting : BaseSorting
{
    /// <summary>
    /// By.
    /// </summary>
    public virtual CompanySortBy By { get; set; } = CompanySortBy.Relevance;
}