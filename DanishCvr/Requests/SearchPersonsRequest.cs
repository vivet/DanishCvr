using DanishCvr.Requests.Models;

namespace DanishCvr.Requests;

/// <summary>
/// Search Persons Request.
/// </summary>
public class SearchPersonsRequest : BaseSearchRequest<SearchPersonCriteria, Paging, PersonSorting>;