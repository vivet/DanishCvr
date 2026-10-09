namespace DanishCvr.Responses.Models;

/// <summary>
/// Liable Participant.
/// </summary>
public class LiableParticipant : BaseRelation
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