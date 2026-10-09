using System.Collections.Generic;

namespace DanishCvr.Responses;

/// <summary>
/// Base Auto Complete Results Response (abstract).
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class BaseAutoCompleteResultsResponse<TResult> : BaseResponse
{
    /// <summary>
    /// Results.
    /// </summary>
    public IEnumerable<TResult> Results { get; set; } = new List<TResult>();
}