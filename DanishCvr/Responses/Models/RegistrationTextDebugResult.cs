namespace DanishCvr.Responses.Models;

/// <summary>
/// Registration Text Result.
/// </summary>
public class RegistrationTextDebugResult : BaseDebugResult
{
    /// <summary>
    /// Registration Text.
    /// </summary>
    public virtual RegistrationTextResult RegistrationText { get; set; }
}