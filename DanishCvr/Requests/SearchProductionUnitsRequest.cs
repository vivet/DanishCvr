using DanishCvr.Requests.Models;

namespace DanishCvr.Requests;

/// <summary>
/// Search Production Units Request.
/// </summary>
public class SearchProductionUnitsRequest : BaseSearchRequest<SearchProductionUnitCriteria, Paging, ProductionUnitSorting>;