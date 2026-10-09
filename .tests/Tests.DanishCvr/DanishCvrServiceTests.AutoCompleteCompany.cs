using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Requests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task AutoCompleteCompanyTest()
    {
        const string NAME = "Vivet";

        var response = await this.DanishCvrService
            .AutoCompleteCompaniesAsync(new AutoCompleteCompaniesRequest
            {
                Names =
                {
                    Name = NAME
                }
            });

        Assert.IsNotNull(response);
        Assert.AreEqual(10, response.Results.Count());
    }

    [TestMethod]
    public async Task AutoCompleteCompanyWhenAlternativeNameTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task AutoCompleteCompanyWhenAlternativeNameAndNotIncludedTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task AutoCompleteCompanyWhenHistoricNameTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task AutoCompleteCompanyWhenHistoricNameAndNotIncludedTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task AutoCompleteCompanyWhenIsActiveTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }
}