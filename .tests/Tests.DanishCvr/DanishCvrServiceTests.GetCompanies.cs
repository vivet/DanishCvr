using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetCompanyWithRegistrationNumbersTest()
    {
        const string CVR_NUMMER_1 = "10198054";
        const string CVR_NUMMER_2 = "21705500";
        const string CVR_NUMMER_3 = "10191246";

        var response = await this.DanishCvrService
            .GetCompaniesAsync([CVR_NUMMER_1, CVR_NUMMER_2, CVR_NUMMER_3]);

        Assert.IsNotNull(response);

        Assert.IsTrue(response.Results.Any(x => x.Company.RegistrationNumber == CVR_NUMMER_1));
        Assert.IsTrue(response.Results.Any(x => x.Company.RegistrationNumber == CVR_NUMMER_2));
        Assert.IsTrue(response.Results.Any(x => x.Company.RegistrationNumber == CVR_NUMMER_3));
    }
}