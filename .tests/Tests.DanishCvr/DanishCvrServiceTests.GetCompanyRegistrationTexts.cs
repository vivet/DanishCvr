using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetRegistrationTextsTest()
    {
        const string CVR_NUMMER = "43804146";

        var response = await this.DanishCvrService
            .GetCompanyRegistrationTextsAsync(CVR_NUMMER);

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(4, results.Length);

        var result = results.FirstOrDefault();
        Assert.IsNotNull(result);
        Assert.AreEqual("36998871", result.RegistrationText.ExternalId);
        Assert.IsNotNull(result.RegistrationText.RegisteredAt);
        Assert.IsNotNull(result.RegistrationText.PublishedAt);
        Assert.AreEqual("43804146", result.RegistrationText.RegistrationNumber);
        Assert.AreEqual("Vutal ApS", result.RegistrationText.Name);
        Assert.AreEqual("1200", result.RegistrationText.PostalCode);
        Assert.IsNotNull(result.RegistrationText.Text);
        Assert.IsNotNull(result.RegistrationText.UpdatedAt);
        Assert.AreEqual(1, result.RegistrationText.Statuses.Count());
    }

    [TestMethod]
    public async Task GetRegistrationTextsHasCorrectOrderingTest()
    {
        const string CVR_NUMMER = "43804146";

        var response = await this.DanishCvrService
            .GetCompanyRegistrationTextsAsync(CVR_NUMMER);

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(4, results.Length);

        var result = results
            .Select(x => x.RegistrationText.RegisteredAt)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1];
            var current = result[i];

            Assert.IsTrue(current <= previous);
        }
    }
}