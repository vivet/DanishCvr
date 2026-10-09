namespace DanishCvr.Responses.Models;

/// <summary>
/// Person Auto Complete Result.
/// </summary>
public class PersonAutoCompleteResult
{
    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Title.
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public virtual string City { get; set; }

    /// <summary>
    /// Postal Code.
    /// </summary>
    public virtual string PostalCode { get; set; }
}