using System.Collections.Generic;

namespace DanishCvr.Models;

/// <summary>
/// Fusion Spaltning.
/// </summary>
public class FusionSpaltning
{
    /// <summary>
    /// Enheds Nummer Organisation.
    /// </summary>
    public virtual string EnhedsNummerOrganisation { get; set; }

    /// <summary>
    /// Organisations Navn.
    /// </summary>
    public virtual IEnumerable<Navne> OrganisationsNavn { get; set; } = new List<Navne>();

    /// <summary>
    /// Indgaaende.
    /// </summary>
    public virtual IEnumerable<Attributter> Indgaaende { get; set; } = new List<Attributter>();

    /// <summary>
    /// Udgaaende.
    /// </summary>
    public virtual IEnumerable<Attributter> Udgaaende { get; set; } = new List<Attributter>();
}