using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Interfaces;
using DanishCvr.Models.Debug;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

[TestClass]
public class DanishCvrBatchServiceTests : BaseTests
{
    private IDanishCvrBatchService DanishCvrBatchService => this.serviceProvider.GetService<IDanishCvrBatchService>();

    [TestMethod]
    public async Task SearchAllTest()
    {
        await Task.CompletedTask;

        const int MAX_ITERATIONS = 1;
        const int SKIP_ITERATIONS = 0;

        var debugInformation = await this.DanishCvrBatchService
            .SearchAllAsync(x =>
            {
                Debug.WriteLine($"callback: {x.Count()}");
            }, MAX_ITERATIONS, SKIP_ITERATIONS, 2);

        this.PrintDebugInformation(debugInformation);
    }

    private void PrintDebugInformation(DebugInformation debugInformation)
    {
        if (debugInformation == null) 
            throw new ArgumentNullException(nameof(debugInformation));
        
        foreach (var debugHovedType in debugInformation.HovedTyper)
        {
            Debug.WriteLine($"{debugHovedType.Type}: {debugHovedType.Count}");

            foreach (var debugAttribute in debugHovedType.Attributes)
            {
                var registrationNumbers = string.Join(',', debugAttribute.SampleRegistrationNumbers);

                Debug.WriteLine($"    {debugAttribute.Value}: {debugAttribute.Count} ({registrationNumbers})");
            }
        }
    }
}