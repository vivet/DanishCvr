using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DanishCvr.Consts;
using DanishCvr.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DanishCvr.Models.Debug;
using DanishCvr.Models;
using DanishCvr.Responses.Models;
using DanishCvr.Requests.Models;

namespace DanishCvr;

/// <summary>
/// Danish Cvr Batch Service.
/// </summary>
public class DanishCvrBatchService : IDanishCvrBatchService
{
    private ILogger Logger { get; }
    private IDanishCvrService DanishCvrService { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="danishCvrService">The <see cref="DanishCvrOptions"/>.</param>
    public DanishCvrBatchService(ILogger logger, IDanishCvrService danishCvrService)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.DanishCvrService = danishCvrService ?? throw new ArgumentNullException(nameof(danishCvrService));
    }

    /// <inheritdoc />
    public virtual async Task<DebugInformation> SearchAllAsync(Action<IEnumerable<CompanyDebugResult>> postAction, int maxInterations, int skipInterations, int maxParallelism, CancellationToken cancellationToken = default)
    {
        if (postAction == null) 
            throw new ArgumentNullException(nameof(postAction));

        const int RETRIES = 3;

        var combinations = this.GetCombinations(maxInterations, skipInterations);

        var debugInformation = new DebugInformation();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(combinations, parallelOptions, async (combination, innerCancellationToken) =>
        {
            var retries = 0;
            var success = false;

            while (!success)
            {
                try
                {
                    var response = await this.DanishCvrService
                        .GetCompaniesAsync(combination, new Paging { Count = 2500 }, innerCancellationToken);

                    var results = response.Results
                        .ToArray();

                    if (results.Length >= 2500)
                    {
                        this.Logger
                            .LogWarning($"Combination: '{combination}' returned 2500 or more results.");
                    }

                    this.UpdateDebugInformation(results, ref debugInformation);

                    postAction(results);

                    retries = 0;
                    success = true;
                }
                catch (Exception ex)
                {
                    retries++;

                    if (retries == RETRIES)
                    {
                        this.Logger
                            .LogError(ex, $"Search: '{combination}*' FAILED after {retries} retries.");

                        return;
                    }
                }
            }
        });

        return debugInformation;
    }

    private List<string> GetCombinations(int maxInterations, int skipInterations)
    {
        var combinations = new List<string>();

        for (var firstDigit = 1; firstDigit <= 9; firstDigit++)
        {
            for (var i = skipInterations; i < maxInterations; i++)
            {
                var combination = firstDigit + i.ToString("D3");

                combinations
                    .Add(combination);
            }
        }

        return combinations;
    }
    private void UpdateDebugInformation(IEnumerable<CompanyDebugResult> responses, ref DebugInformation debugInformation)
    {
        if (responses == null)
            throw new ArgumentNullException(nameof(responses));

        if (debugInformation == null) 
            throw new ArgumentNullException(nameof(debugInformation));

        if (!Debugger.IsAttached)
        {
            return;
        }

        var virksomheder = responses
            .Select(x => JsonConvert.DeserializeObject<VrVirksomhed>(x.RawJson))
            .ToArray();

        var debugAttributes = virksomheder
            .SelectMany(x => x.Attributter
                .Select(y => new DebugAttribute
                {
                    Value = y.Type,
                    Count = 1,
                    SampleRegistrationNumbers =
                    [
                        x.CvrNummer
                    ]
                }));

        foreach (var debugAttribute in debugAttributes)
        {
            var attribute = debugInformation.Attributes
                .FirstOrDefault(y => y.Value == debugAttribute.Value);

            if (attribute == null)
            {
                debugInformation.Attributes
                    .Add(debugAttribute);
            }
            else
            {
                attribute.Count++;

                if (attribute.SampleRegistrationNumbers.Count < 10)
                {
                    foreach (var debugAttributeSampleRegistrationNumber in debugAttribute.SampleRegistrationNumbers)
                    {
                        attribute.SampleRegistrationNumbers
                            .Add(debugAttributeSampleRegistrationNumber);
                    }
                }
            }
        }

        var debugHovedTypes = virksomheder
            .SelectMany(x => x.DeltagerRelation
                .SelectMany(y => y.Organisationer
                    .Select(z =>
                    {
                        var debugHovedType = new DebugHovedType
                        {
                            Type = z.HovedType,
                            Count = 1
                        };

                        var attributes = z.Attributter
                            .Where(a => a.Type == AttributTyper.FUNKTION)
                            .SelectMany(a => a.Vaerdier)
                            .DistinctBy(a => a.Vaerdi)
                            .Select(a => new DebugAttribute
                            {
                                Value = a.Vaerdi,
                                Count = 1,
                                SampleRegistrationNumbers =
                                [
                                    x.CvrNummer
                                ]
                            });

                        foreach (var debugAttribute in attributes)
                        {
                            debugHovedType.Attributes
                                .Add(debugAttribute);
                        }

                        return debugHovedType;
                    })));

        foreach (var debugHovedType in debugHovedTypes)
        {
            var hovedType = debugInformation.HovedTyper
                .FirstOrDefault(y => y.Type == debugHovedType.Type);

            if (hovedType == null)
            {
                debugInformation.HovedTyper
                    .Add(debugHovedType);
            }
            else
            {
                hovedType.Count++;

                foreach (var debugAttribute in debugHovedType.Attributes)
                {
                    var attribute = hovedType.Attributes
                        .FirstOrDefault(x => x.Value == debugAttribute.Value);

                    if (attribute == null)
                    {
                        hovedType.Attributes
                            .Add(debugAttribute);
                    }
                    else
                    {
                        attribute.Count++;

                        if (attribute.SampleRegistrationNumbers.Count < 10)
                        {
                            if (!attribute.SampleRegistrationNumbers.Any(x => debugAttribute.SampleRegistrationNumbers.Any(y => y == x))) 
                            {
                                foreach (var debugAttributeSampleRegistrationNumber in debugAttribute.SampleRegistrationNumbers)
                                {
                                    attribute.SampleRegistrationNumbers
                                        .Add(debugAttributeSampleRegistrationNumber);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}