namespace DanishCvr.Requests.Models;

/// <summary>
/// Names With Alternative.
/// </summary>
public class NamesWithAlternative : Names
{
    /// <summary>
    /// Include Alternative Names.
    /// </summary>
    public virtual bool IncludeAlternativeNames { get; set; } = true;
}