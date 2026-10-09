namespace DanishCvr.Responses.Models;

/// <summary>
/// Company Debug Result.
/// </summary>
public class CompanyDebugResult : BaseDebugResult
{
    /// <summary>
    /// Company.
    /// </summary>
    public virtual CompanyResult Company { get; set; }
}