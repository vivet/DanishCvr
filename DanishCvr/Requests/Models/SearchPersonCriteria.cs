using System.ComponentModel.DataAnnotations;
using DanishCvr.Types;

namespace DanishCvr.Requests.Models;

/// <summary>
/// Search Person Criteria.
/// </summary>
public class SearchPersonCriteria
{
    /// <summary>
    /// Name.
    /// </summary>
    [MinLength(3)]
    public virtual string Name { get; set; }

    /// <summary>
    /// Within.
    /// </summary>
    public virtual Within Within { get; set; }
}