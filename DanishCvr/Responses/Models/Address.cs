using System;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Address.
/// </summary>
public class Address : DateAndPeriod
{
    /// <summary>
    /// Country.
    /// </summary>
    public virtual string Country { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public virtual City City { get; set; } = new();

    /// <summary>
    /// Street Name.
    /// </summary>
    public virtual string StreetName { get; set; }

    /// <summary>
    /// Co Street Name.
    /// </summary>
    public virtual string CoStreetName { get; set; }

    /// <summary>
    /// Postbox.
    /// </summary>
    public virtual string Postbox { get; set; }

    /// <summary>
    /// House Number From.
    /// </summary>
    public virtual string HouseNumberFrom { get; set; }

    /// <summary>
    /// House Number To.
    /// </summary>
    public virtual string HouseNumberTo { get; set; }

    /// <summary>
    /// Letter From.
    /// </summary>
    public virtual string LetterFrom { get; set; }

    /// <summary>
    /// Letter To.
    /// </summary>
    public virtual string LetterTo { get; set; }

    /// <summary>
    /// Floor.
    /// </summary>
    public virtual string Floor { get; set; }

    /// <summary>
    /// Door.
    /// </summary>
    public virtual string Door { get; set; }

    /// <summary>
    /// Is Unlisted.
    /// </summary>
    public virtual bool IsUnlisted { get; set; } = false;

    /// <summary>
    /// Validated At.
    /// </summary>
    public virtual DateTimeOffset? ValidatedAt { get; set; }

    /// <summary>
    /// Validated At.
    /// </summary>
    public virtual bool? IsAddressValidationDiscontinued { get; set; }
}