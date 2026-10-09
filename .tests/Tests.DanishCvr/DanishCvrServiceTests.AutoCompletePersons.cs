using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Requests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task AutoCompletePersonTest()
    {
        const string NAME = "Michael Viv";

        var response = await this.DanishCvrService
            .AutoCompletePersonsAsync(new AutoCompletePersonsRequest
            {
                Name = NAME
            });

        Assert.IsNotNull(response);
        Assert.AreEqual(4, response.Results.Count());
    }
}