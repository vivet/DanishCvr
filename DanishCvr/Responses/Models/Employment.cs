namespace DanishCvr.Responses.Models;

/// <summary>
/// Employment.
/// </summary>
public class Employment
{
    /// <summary>
    /// Employees.
    /// </summary>
    public virtual Employeeses Employees { get; set; }

    /// <summary>
    /// Managers.
    /// </summary>
    public virtual Managers Managers { get; set; }
}