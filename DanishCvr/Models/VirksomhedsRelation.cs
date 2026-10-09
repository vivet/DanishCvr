namespace DanishCvr.Models;

/// <summary>
/// Virksomheds Relation.
/// </summary>
public class VirksomhedsRelation : DatoOgPeriode
{
    /// <summary>
    /// Cvr Nummer.
    /// </summary>
    public virtual string CvrNummer { get; set; }
}