namespace DanishCvr.Responses;

/// <summary>
/// Base Response (abstract).
/// </summary>
public abstract class BaseResultResponse<TResult> : BaseResponse
{
    /// <summary>
    /// Result.
    /// </summary>
    public TResult Result { get; set; }
}