using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Responses.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetPersonTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
    }

    [TestMethod]
    public async Task GetPersonWhenNameTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Person.Name);
        Assert.AreEqual("Simon Bækgaard Kristoffersen", response.Result.Person.Name.Value);
    }

    [TestMethod]
    public async Task GetPersonWhenAddressesTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Person.Addresses.Current);

        Assert.AreEqual("DK", response.Result.Person.Addresses.Current.Country);
        Assert.AreEqual("København S", response.Result.Person.Addresses.Current.City.Name);
        Assert.AreEqual("2300", response.Result.Person.Addresses.Current.City.PostalCode);
        Assert.AreEqual("J.H. Deuntzers Gade", response.Result.Person.Addresses.Current.StreetName);
        Assert.AreEqual("4", response.Result.Person.Addresses.Current.HouseNumberFrom);

        Assert.AreEqual("DK", response.Result.Person.Addresses.Latest.Country);
        Assert.AreEqual("København S", response.Result.Person.Addresses.Latest.City.Name);
        Assert.AreEqual("2300", response.Result.Person.Addresses.Latest.City.PostalCode);
        Assert.AreEqual("J.H. Deuntzers Gade", response.Result.Person.Addresses.Latest.StreetName);
        Assert.AreEqual("4", response.Result.Person.Addresses.Latest.HouseNumberFrom);

        Assert.AreEqual(1, response.Result.Person.Addresses.HistoricAddresses.Count());
    }

    [TestMethod]
    public async Task GetPersonWhenPhoneNumbersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetPersonWhenFaxNumbersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetPersonWhenEmailAddressesTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetPersonyWhenTitleTest()
    {
        const string PERSON_ID = "4003887725";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("Bestyrelsesmedlem", response.Result.Person.Title);
    }

    [TestMethod]
    public async Task GetPersonWhenEntityTypeTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.AreEqual(EntityType.Person, response.Result.Person.EntityType);
    }

    [TestMethod]
    public async Task GetPersonWhenImportErrorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetPersonWhenRegistrationErrorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetPersonWhenExternalIdTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsNotNull(response.Result.Person.ExternalId);
    }

    [TestMethod]
    public async Task GetPersonWhenUpdatedAtTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Person.UpdatedAt);
    }

    [TestMethod]
    public async Task GetPersonWhenCompaniesTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetPersonAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(4, response.Result.Person.Companies.Count());

        var company = response.Result.Person.Companies.FirstOrDefault();
        Assert.IsNotNull(company);
        Assert.AreEqual(5, company.ActiveRoles.Count());
    }
}