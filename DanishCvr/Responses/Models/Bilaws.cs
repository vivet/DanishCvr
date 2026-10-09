using DanishCvr.Types;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Bilaws.
/// </summary>
public class Bilaws : Period
{
    /// <summary>
    /// Notes.
    /// </summary>
    public virtual string Notes { get; set; }
}