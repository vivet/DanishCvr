namespace DanishCvr.Responses.Models;

/// <summary>
/// Errors.
/// </summary>
public class Errors
{
    /// <summary>
    /// Has Import Error.
    /// </summary>
    public virtual bool HasImportErrors { get; set; }

    /// <summary>
    /// Has Registration Error.
    /// </summary>
    public virtual bool HasRegistrationErrors { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public virtual string Description { get; set; }
}