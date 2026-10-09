using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DanishCvr.Models.Debug;
using DanishCvr.Responses.Models;

namespace DanishCvr.Interfaces;

/// <summary>
/// Danish Cvr Batch Service Interface.
/// </summary>
public interface IDanishCvrBatchService
{
    /// <summary>
    /// Search All Async.
    /// </summary>
    /// <param name="postAction">The post action to execute.</param>
    /// <param name="maxInterations">The max iterations.</param>
    /// <param name="skipInterations">The skip interations</param>
    /// <param name="maxParallelism">The max concurrent searches.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DebugInformation"/>.</returns>
    Task<DebugInformation> SearchAllAsync(Action<IEnumerable<CompanyDebugResult>> postAction, int maxInterations, int skipInterations, int maxParallelism, CancellationToken cancellationToken = default);
}