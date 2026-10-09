using DanishCvr.Requests.Models;

namespace DanishCvr.Requests;

/// <summary>
/// Search Companies Request.
/// </summary>
public class SearchCompaniesRequest : BaseSearchRequest<SearchCompaniesCriteria, Paging, CompanySorting>;