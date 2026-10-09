using System.Linq;
using DanishCvr.Helpers;
using DanishCvr.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr.Helpers;

[TestClass]
public class PostalCodesTests : BaseTests
{
    [TestMethod]
    public void GetPostalCodesTest()
    {
        var postalCodes = PostalCodes.GetPostalCodes();

        Assert.AreEqual(1089, postalCodes.Count());
    }

    [TestMethod]
    public void GetPostalCodesWithinTest()
    {
        var within = new Within
        {
            Location = new Location
            {
                Latitude = 55.65941452,
                Longitude = 12.49869399
            },
            Radius = 1
        };

        var postalCodes = PostalCodes.GetPostalCodesWithin(within);

        Assert.AreEqual(3, postalCodes.Count());
    }
}