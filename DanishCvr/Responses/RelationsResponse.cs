using DanishCvr.Responses.Models;

namespace DanishCvr.Responses;

/// <summary>
/// Relations Response.
/// </summary>
public class RelationsResponse : BaseResultResponse<RelationDebugResult>
{
    /// <summary>
    /// Total Results.
    /// Not the returned results, but the total number of results,
    /// that matched the criteria.
    /// </summary>
    public long TotalResults { get; set; }
}