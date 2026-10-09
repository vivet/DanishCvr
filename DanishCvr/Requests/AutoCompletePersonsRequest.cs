using System.ComponentModel.DataAnnotations;

namespace DanishCvr.Requests;

/// <summary>
/// Auto Complete Persons Request.
/// </summary>
public class AutoCompletePersonsRequest 
{
    /// <summary>
    /// Name.
    /// </summary>
    [Required]
    [MinLength(3)]
    public virtual string Name { get; set; }
}