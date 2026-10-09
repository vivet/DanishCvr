using System;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Bilaws Approval.
/// </summary>
public class BilawsApproval
{
    /// <summary>
    /// Authority.
    /// </summary>
    public virtual string Authority { get; set; }

    /// <summary>
    /// Approved At.
    /// </summary>
    public virtual DateOnly? ApprovedAt { get; set; }
}