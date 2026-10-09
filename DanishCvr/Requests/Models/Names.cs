using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Names.
/// </summary>
public class Names
{
    /// <summary>
    /// Name.
    /// </summary>
    [MinLength(3)]
    public virtual string Name { get; set; }

    /// <summary>
    /// Include Historic Names.
    /// </summary>
    public virtual bool IncludeHistoricNames { get; set; } = false;
}