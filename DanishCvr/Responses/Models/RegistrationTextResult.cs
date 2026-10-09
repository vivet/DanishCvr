using System;
using System.Collections.Generic;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Registration Text.
/// </summary>
public class RegistrationTextResult
{
    /// <summary>
    /// External Id.
    /// </summary>
    public virtual string ExternalId { get; set; }

    /// <summary>
    /// Registered At.
    /// </summary>
    public virtual DateTimeOffset? RegisteredAt { get; set; }

    /// <summary>
    /// Published At.
    /// </summary>
    public virtual DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Registration Number.
    /// </summary>
    public virtual string RegistrationNumber { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Address.
    /// </summary>
    public virtual string Address { get; set; }

    /// <summary>
    /// Postal Code.
    /// </summary>
    public virtual string PostalCode { get; set; }

    /// <summary>
    /// Text.
    /// </summary>
    public virtual string Text { get; set; }

    /// <summary>
    /// Updated At.
    /// </summary>
    public virtual DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Statuses.
    /// </summary>
    public virtual IEnumerable<string> Statuses { get; set; } = new List<string>();
}