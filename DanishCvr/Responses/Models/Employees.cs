using System;
using DanishCvr.Types;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Employees.
/// </summary>
public class Employees
{
    /// <summary>
    /// Year.
    /// </summary>
    public virtual Period Period { get; set; } = new();

    /// <summary>
    /// Number Of Employees.
    /// </summary>
    public virtual int? NumberOfEmployees { get; set; }

    /// <summary>
    /// Number Of Fulltime Positions.
    /// </summary>
    public virtual double? NumberOfFulltimePositions { get; set; }

    /// <summary>
    /// Updated At.
    /// </summary>
    public virtual DateTimeOffset? UpdatedAt { get; set; }
}