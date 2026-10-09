namespace DanishCvr.Responses.Models;

/// <summary>
/// Person Debug Result.
/// </summary>
public class PersonDebugResult : BaseDebugResult
{
    /// <summary>
    /// Person.
    /// </summary>
    public virtual PersonResult Person { get; set; }
}