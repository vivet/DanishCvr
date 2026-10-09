namespace DanishCvr.Responses.Models;

/// <summary>
/// Relation Liable Participant.
/// </summary>
public class RelationLiableParticipant : BaseRelationRole
{
    /// <summary>
    /// Role.
    /// </summary>
    public virtual string Role { get; set; }

    /// <summary>
    /// Election Method.
    /// </summary>
    public virtual string ElectionMethod { get; set; }

    /// <summary>
    /// Registered Capital.
    /// </summary>
    public virtual RegisteredCapital RegisteredCapital { get; set; }
}