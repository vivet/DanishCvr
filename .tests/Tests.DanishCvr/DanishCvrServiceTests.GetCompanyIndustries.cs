using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetCompanyIndustriesTest()
    {
        var response = await this.DanishCvrService
            .GetCompanyIndustriesAsync();

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(738, results.Length);
    }

    [TestMethod]
    public async Task GetCompanyIndustriesHasCorrectOrderingTest()
    {
        var response = await this.DanishCvrService
            .GetCompanyIndustriesAsync();

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

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