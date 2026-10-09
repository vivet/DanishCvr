using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Names Required.
/// </summary>
public class NamesRequired
{
    /// <summary>
    /// Name.
    /// </summary>
    [Required]
    [MinLength(3)]
    public virtual string Name { get; set; }

    /// <summary>
    /// Include Historic Names.
    /// </summary>
    public virtual bool IncludeHistoricNames { get; set; } = false;
}