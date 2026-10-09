using System;
using DanishCvr.Types;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Date And Period.
/// </summary>
public class DateAndPeriod
{
    /// <summary>
    /// Period.
    /// </summary>
    public virtual Period Period { get; set; } = new();

    /// <summary>
    /// Updated At.
    /// </summary>
    public virtual DateTimeOffset? UpdatedAt { get; set; }
}