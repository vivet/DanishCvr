using DanishCvr.Requests.Models.Enums;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Production Unit Sorting.
/// </summary>
public class ProductionUnitSorting : BaseSorting
{
    /// <summary>
    /// By.
    /// </summary>
    public virtual ProductionUnitSortBy By { get; set; } = ProductionUnitSortBy.Relevance;
}