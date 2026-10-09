namespace DanishCvr.Requests.Models;

/// <summary>
/// Names With Alternative Required.
/// </summary>
public class NamesWithAlternativeRequired : NamesRequired
{
    /// <summary>
    /// Include Alternative Names.
    /// </summary>
    public virtual bool IncludeAlternativeNames { get; set; } = true;
}