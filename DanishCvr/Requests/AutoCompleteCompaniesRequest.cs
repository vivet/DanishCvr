using DanishCvr.Requests.Models;

namespace DanishCvr.Requests;

/// <summary>
/// Auto Complete Companies Request.
/// </summary>
public class AutoCompleteCompaniesRequest 
{
    /// <summary>
    /// Names.
    /// </summary>
    public virtual NamesWithAlternativeRequired Names { get; set; } = new();

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool? IsActive { get; set; }
}