using System.Collections.Generic;

namespace DanishCvr.Responses;

/// <summary>
/// Base Response (abstract).
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class BaseResultsResponse<TResult> : BaseResponse
{
    /// <summary>
    /// Results.
    /// </summary>
    public IEnumerable<TResult> Results { get; set; } = new List<TResult>();

    /// <summary>
    /// Total Results.
    /// Not the returned results, but the total number of results,
    /// that matched the criteria.
    /// </summary>
    public long TotalResults { get; set; }
}