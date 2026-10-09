using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetCompanyWithExternalIdTest()
    {
        const string EXTERNAL_ID = "4004308865";

        var response = await this.DanishCvrService
            .GetCompanyByExternalIdAsync(EXTERNAL_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.AreEqual(EXTERNAL_ID, response.Result.Company.ExternalId);
    }
}