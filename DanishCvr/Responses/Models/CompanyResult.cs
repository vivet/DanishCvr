using System;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Company Result.
/// </summary>
public class CompanyResult : BaseCompany
{
    /// <summary>
    /// Names.
    /// </summary>
    public virtual Names Names { get; set; }

    /// <summary>
    /// Alternative Names.
    /// </summary>
    public virtual AlternativeNamesses AlternativeNames { get; set; }

    /// <summary>
    /// Addresses.
    /// </summary>
    public virtual Addresses Addresses { get; set; }

    /// <summary>
    /// Phone Numbers.
    /// </summary>
    public virtual PhoneNumbers PhoneNumbers { get; set; }

    /// <summary>
    /// Fax Numbers.
    /// </summary>
    public virtual FaxNumbers FaxNumbers { get; set; }

    /// <summary>
    /// Email Addresses.
    /// </summary>
    public virtual EmailAddresses EmailAddresses { get; set; }

    /// <summary>
    /// Websites.
    /// </summary>
    public virtual Websites Websites { get; set; }

    /// <summary>
    /// Purpose.
    /// </summary>
    public virtual Purposes Purpose { get; set; }

    /// <summary>
    /// Industry.
    /// </summary>
    public virtual Industries Industry { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    public virtual BusinessTypes Type { get; set; }

    /// <summary>
    /// Status.
    /// </summary>
    public virtual Statuses Status { get; set; }

    /// <summary>
    /// Employment.
    /// </summary>
    public virtual Employment Employment { get; set; }

    /// <summary>
    /// Registered Capital.
    /// </summary>
    public virtual RegisteredCapitals RegisteredCapital { get; set; }

    /// <summary>
    /// Financial Years.
    /// </summary>
    public virtual FinancialYears FinancialYear { get; set; }

    /// <summary>
    /// Anti Money Laundering.
    /// </summary>
    public virtual AntiMoneyLaundering AntiMoneyLaundering { get; set; }

    /// <summary>
    /// Auditing.
    /// </summary>
    public virtual Auditing Auditing { get; set; }

    /// <summary>
    /// Authority.
    /// </summary>
    public virtual Authority Authority { get; set; }

    /// <summary>
    /// Governance.
    /// </summary>
    public virtual Governance Governance { get; set; }

    /// <summary>
    /// Ownership.
    /// </summary>
    public virtual Ownership Ownership { get; set; }

    /// <summary>
    /// Bilaws.
    /// </summary>
    public virtual Bilawsses Bilaws { get; set; }

    /// <summary>
    /// Auditor Registration.
    /// </summary>
    public virtual AuditorRegistration AuditorRegistration { get; set; }

    /// <summary>
    /// Production Units.
    /// </summary>
    public virtual ProductionUnits ProductionUnits { get; set; }

    /// <summary>
    /// Effective Started At.
    /// </summary>
    public virtual DateOnly? EffectiveStartedAt { get; set; }

    /// <summary>
    /// Commercial Fund Approved At.
    /// </summary>
    public virtual DateOnly? CommercialFundApprovedAt { get; set; }

    /// <summary>
    /// Errors.
    /// </summary>
    public virtual Errors Errors { get; set; }
}