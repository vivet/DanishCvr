using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Requests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetProductionUnitsTest()
    {
        const string REGISTRATION_NUMBER = "13612870";

        var response = await this.DanishCvrService
            .GetProductionUnitsAsync(REGISTRATION_NUMBER);

        Assert.IsNotNull(response);
        Assert.AreEqual(2, response.Results.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitsWhenPagingTest()
    {
        const string REGISTRATION_NUMBER = "13612870";

        var response = await this.DanishCvrService
            .GetProductionUnitsAsync(REGISTRATION_NUMBER, true, new Paging { Count = 1, Skip = 1 });

        Assert.IsNotNull(response);
        Assert.AreEqual(1, response.Results.Count());

        var result = response.Results.FirstOrDefault();
        Assert.IsNotNull(result);
        Assert.AreEqual("1010866444", result.ProductionUnit.ProductionUnitNumber);
    }
}