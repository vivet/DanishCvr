namespace DanishCvr.Responses.Models;

/// <summary>
/// Company Search Debug Result.
/// </summary>
public class CompanySearchDebugResult : BaseDebugResult
{
    /// <summary>
    /// Company.
    /// </summary>
    public virtual CompanySearchResult Company { get; set; }
}