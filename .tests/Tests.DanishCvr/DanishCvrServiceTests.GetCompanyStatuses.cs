using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetCompanyStatusesTest()
    {
        var response = await this.DanishCvrService
            .GetCompanyStatusesAsync();

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(18, results.Length);
    }

    [TestMethod]
    public async Task GetCompanyStatusesHasCorrectOrderingTest()
    {
        var response = await this.DanishCvrService
            .GetCompanyStatusesAsync();

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

        var result = results
            .Select(x => x.Text)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) <= 0, $"The list is not in correct order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }
}