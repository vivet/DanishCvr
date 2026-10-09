using System;

namespace DanishCvr;

/// <summary>
/// Danish Cvr Options.
/// </summary>
public class DanishCvrOptions
{
    /// <summary>
    /// Section Name.
    /// </summary>
    public const string SECTION_NAME = "DanishCvr";

    /// <summary>
    /// Username.
    /// </summary>
    public virtual string Username { get; set; }

    /// <summary>
    /// Password.
    /// </summary>
    public virtual string Password { get; set; }

    /// <summary>
    /// Request Timeout.
    /// </summary>
    public virtual TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Include Raw Json.
    /// When true, the raw json of each result is populated also when no debugger is attached.
    /// </summary>
    public virtual bool IncludeRawJson { get; set; }
}