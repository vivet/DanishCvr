using System.Text.RegularExpressions;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Company Auto Complete Result.
/// </summary>
public class CompanyAutoCompleteResult
{
    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }

    /// <summary>
    /// Latest Name.
    /// </summary>
    public virtual string LatestName { get; set; }

    /// <summary>
    /// Match.
    /// </summary>
    public virtual CompanyAutoCompleteMatch Match { get; set; } = new();

    /// <summary>
    /// Business Type Abbreviation.
    /// </summary>
    public virtual string BusinessTypeAbbreviation { get; set; }

    /// <summary>
    /// City Name.
    /// </summary>
    public virtual string CityName { get; set; }

    /// <summary>
    /// City Postal Code.
    /// </summary>
    public virtual string CityPostalCode { get; set; }

    /// <summary>
    /// Is Active.
    /// </summary>
    public virtual bool IsActive { get; set; }
}