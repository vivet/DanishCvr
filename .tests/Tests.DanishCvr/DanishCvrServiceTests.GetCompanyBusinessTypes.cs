using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetCompanyBusinessTypesTest()
    {
        var response = await this.DanishCvrService
            .GetCompanyBusinessTypesAsync();

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(41, results.Length);
    }

    [TestMethod]
    public async Task GetCompanyBusinessTypesHasCorrectOrderingTest()
    {
        var response = await this.DanishCvrService
            .GetCompanyBusinessTypesAsync();

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(41, results.Length);

        var result = results
            .Select(x => x.Code)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = int.Parse(result[i - 1]);
            var current = int.Parse(result[i]);

            Assert.IsTrue(current > previous);
        }
    }
}