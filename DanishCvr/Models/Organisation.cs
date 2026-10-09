using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Organisation.
/// </summary>
public class Organisation
{
    /// <summary>
    /// Enheds Nummer Organisation.
    /// </summary>
    public virtual string EnhedsNummerOrganisation { get; set; }

    /// <summary>
    /// Hoved Type.
    /// </summary>
    public virtual string HovedType { get; set; }

    /// <summary>
    /// Organisations Navn.
    /// </summary>
    public virtual IEnumerable<Navne> OrganisationsNavn { get; set; } = new List<Navne>();

    /// <summary>
    /// Attributter.
    /// </summary>
    public virtual IEnumerable<Attributter> Attributter { get; set; } = new List<Attributter>();

    /// <summary>
    /// Medlems Data.
    /// </summary>
    public virtual IEnumerable<MedlemsData> MedlemsData { get; set; } = new List<MedlemsData>();
}