namespace DanishCvr.Responses.Models;

/// <summary>
/// Person Search Result.
/// </summary>
public class PersonSearchDebugResult : BaseDebugResult
{
    /// <summary>
    /// Person.
    /// </summary>
    public virtual PersonSearchResult Person { get; set; }
}