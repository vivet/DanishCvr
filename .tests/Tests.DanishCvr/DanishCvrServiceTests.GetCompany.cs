using System;
using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Responses.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetCompanyTest()
    {
        const string CVR_NUMMER = "42989894";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(CVR_NUMMER, response.Result.Company.RegistrationNumber);
    }

    [TestMethod]
    public async Task GetCompanyWhenNamesTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Names.Current);
        Assert.AreEqual("MICROSOFT DANMARK ApS", response.Result.Company.Names.Current.Value);
        Assert.AreEqual(new DateOnly(1990, 4, 24), response.Result.Company.Names.Current.Period.From);
        Assert.IsNull(response.Result.Company.Names.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Names.Current.UpdatedAt);

        Assert.IsNotNull(response.Result.Company.Names.Latest);
        Assert.AreEqual("MICROSOFT DANMARK ApS", response.Result.Company.Names.Latest.Value);
        Assert.AreEqual(new DateOnly(1990, 4, 24), response.Result.Company.Names.Latest.Period.From);
        Assert.IsNull(response.Result.Company.Names.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Names.Latest.UpdatedAt);

        Assert.AreEqual(1, response.Result.Company.Names.HistoricNames.Count());
        Assert.AreEqual(13, response.Result.Company.AlternativeNames.Names.Count());
        Assert.AreEqual(0, response.Result.Company.AlternativeNames.HistoricNames.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenNamesAndInactiveTest()
    {
        const string CVR_NUMMER = "37388149";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Names.Current);

        Assert.IsNotNull(response.Result.Company.Names.Latest);
        Assert.AreEqual(".dk Invest Holding ivs", response.Result.Company.Names.Latest.Value);
        Assert.AreEqual(new DateOnly(2016, 01, 22), response.Result.Company.Names.Latest.Period.From);
        Assert.AreEqual(new DateOnly(2017, 11, 17), response.Result.Company.Names.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Names.Latest.UpdatedAt);

        Assert.AreEqual(2, response.Result.Company.Names.HistoricNames.Count());
        Assert.AreEqual(0, response.Result.Company.AlternativeNames.Names.Count());
        Assert.AreEqual(1, response.Result.Company.AlternativeNames.HistoricNames.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenAddressesTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Addresses.Current);

        Assert.AreEqual("DK", response.Result.Company.Addresses.Current.Country);
        Assert.AreEqual("Kgs. Lyngby", response.Result.Company.Addresses.Current.City.Name);
        Assert.AreEqual("2800", response.Result.Company.Addresses.Current.City.PostalCode);
        Assert.AreEqual("Kanalvej", response.Result.Company.Addresses.Current.StreetName);
        Assert.AreEqual("7", response.Result.Company.Addresses.Current.HouseNumberFrom);

        Assert.AreEqual("DK", response.Result.Company.Addresses.Latest.Country);
        Assert.AreEqual("Kgs. Lyngby", response.Result.Company.Addresses.Latest.City.Name);
        Assert.AreEqual("2800", response.Result.Company.Addresses.Latest.City.PostalCode);
        Assert.AreEqual("Kanalvej", response.Result.Company.Addresses.Latest.StreetName);
        Assert.AreEqual("7", response.Result.Company.Addresses.Latest.HouseNumberFrom);

        Assert.AreEqual(2, response.Result.Company.Addresses.HistoricAddresses.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenAddressesAndInactiveTest()
    {
        const string CVR_NUMMER = "37388149";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Addresses.Current);

        Assert.IsNotNull(response.Result.Company.Addresses.Latest);
        Assert.AreEqual("DK", response.Result.Company.Addresses.Latest.Country);
        Assert.AreEqual("Greve", response.Result.Company.Addresses.Latest.City.Name);
        Assert.AreEqual("2670", response.Result.Company.Addresses.Latest.City.PostalCode);
        Assert.AreEqual("Korskildelund", response.Result.Company.Addresses.Latest.StreetName);
        Assert.AreEqual("6", response.Result.Company.Addresses.Latest.HouseNumberFrom);

        Assert.AreEqual(2, response.Result.Company.Addresses.HistoricAddresses.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenPhoneNumbersTest()
    {
        const string CVR_NUMMER = "10192676";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result.Company.PhoneNumbers.Current);
        Assert.AreEqual("49288888", response.Result.Company.PhoneNumbers.Current.Value);
        Assert.IsFalse(response.Result.Company.PhoneNumbers.Current.IsUnlisted);
        Assert.AreEqual(new DateOnly(2017, 9, 1), response.Result.Company.PhoneNumbers.Current.Period.From);
        Assert.IsNull(response.Result.Company.PhoneNumbers.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.PhoneNumbers.Current.UpdatedAt);
        Assert.AreEqual(1, response.Result.Company.PhoneNumbers.HistoricPhoneNumbers.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenPhoneNumbersAndInactiveTest()
    {
        const string CVR_NUMMER = "37114111";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.PhoneNumbers.Current);
        Assert.AreEqual(2, response.Result.Company.PhoneNumbers.HistoricPhoneNumbers.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenFaxNumbersTest()
    {
        const string CVR_NUMMER = "10191246";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.FaxNumbers.Current);
        Assert.AreEqual("35353168", response.Result.Company.FaxNumbers.Current.Value);
        Assert.IsFalse(response.Result.Company.FaxNumbers.Current.IsUnlisted);
        Assert.AreEqual(new DateOnly(2004, 12, 10), response.Result.Company.FaxNumbers.Current.Period.From);
        Assert.IsNull(response.Result.Company.FaxNumbers.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.FaxNumbers.Current.UpdatedAt);
        Assert.AreEqual(0, response.Result.Company.FaxNumbers.HistoricFaxNumbers.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenFaxNumbersAndInactiveTest()
    {
        const string CVR_NUMMER = "37114111";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.FaxNumbers);
    }

    [TestMethod]
    public async Task GetCompanyWhenEmailAddressesTest()
    {
        const string CVR_NUMMER = "10196175";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.EmailAddresses.Current);
        Assert.AreEqual("info@blomfelt.dk", response.Result.Company.EmailAddresses.Current.Value);
        Assert.IsFalse(response.Result.Company.EmailAddresses.Current.IsUnlisted);
        Assert.AreEqual(new DateOnly(2025, 1, 16), response.Result.Company.EmailAddresses.Current.Period.From);
        Assert.IsNull(response.Result.Company.EmailAddresses.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.EmailAddresses.Current.UpdatedAt);
        Assert.AreEqual(2, response.Result.Company.EmailAddresses.HistoricEmailAddresses.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenEmailAddressesAndInactiveTest()
    {
        const string CVR_NUMMER = "37114111";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.EmailAddresses.Current);
        Assert.AreEqual(3, response.Result.Company.EmailAddresses.HistoricEmailAddresses.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenWebsitesTest()
    {
        const string CVR_NUMMER = "10195616";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Websites.Current);
        Assert.AreEqual("www.sanktpetriskole.dk", response.Result.Company.Websites.Current.Value);
        Assert.AreEqual(new DateOnly(2024, 11, 7), response.Result.Company.Websites.Current.Period.From);
        Assert.IsNull(response.Result.Company.Websites.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Websites.Current.UpdatedAt);
        Assert.AreEqual(0, response.Result.Company.Websites.HistoricWebsites.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenWebsitesAndInactiveTest()
    {
        const string CVR_NUMMER = "37114111";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Websites.Current);
        Assert.AreEqual(1, response.Result.Company.Websites.HistoricWebsites.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenPurposeTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Purpose.Current);
        Assert.IsNotNull(response.Result.Company.Purpose.Latest);
        Assert.AreEqual("Selskabets formål er salg af software og tilbehør til computers og service i forbindelse hermed samt anden handel og service", response.Result.Company.Purpose.Current.Value);
        Assert.AreEqual("Selskabets formål er salg af software og tilbehør til computers og service i forbindelse hermed samt anden handel og service", response.Result.Company.Purpose.Latest.Value);
        Assert.AreEqual(0, response.Result.Company.Purpose.HistoricPurposes.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenPurposeAndInActiveTest()
    {
        const string CVR_NUMMER = "37114111";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Purpose.Current);
        Assert.IsNotNull(response.Result.Company.Purpose.Latest);
        Assert.AreEqual("Selskabets formål er at drive en webshop som skal sælge lukus barbergrej til B2C, samt enhver i forbindelse hermed stående virksomhed.", response.Result.Company.Purpose.Latest.Value);
        Assert.AreEqual(1, response.Result.Company.Purpose.HistoricPurposes.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialPurposeTest()
    {
        const string CVR_NUMMER = "12626509";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose);

        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose.Current);
        Assert.AreEqual("PI - Bank", response.Result.Company.Purpose.FinancialPurpose.Current.Value);
        Assert.AreEqual(new DateOnly(1986, 1, 2), response.Result.Company.Purpose.FinancialPurpose.Current.Period.From);
        Assert.IsNull(response.Result.Company.Purpose.FinancialPurpose.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose.Current.UpdatedAt);

        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose.Latest);
        Assert.AreEqual("PI - Bank", response.Result.Company.Purpose.FinancialPurpose.Latest.Value);
        Assert.AreEqual(new DateOnly(1986, 1, 2), response.Result.Company.Purpose.FinancialPurpose.Current.Period.From);
        Assert.IsNull(response.Result.Company.Purpose.FinancialPurpose.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose.Latest.UpdatedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialPurposeAndInactiveTest()
    {
        const string CVR_NUMMER = "12718500";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose);
        Assert.IsNull(response.Result.Company.Purpose.FinancialPurpose.Current);

        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose.Latest);
        Assert.AreEqual("Pantebrselsk", response.Result.Company.Purpose.FinancialPurpose.Latest.Value);
        Assert.AreEqual(new DateOnly(1989, 2, 10), response.Result.Company.Purpose.FinancialPurpose.Latest.Period.From);
        Assert.AreEqual(new DateOnly(2015, 6, 25), response.Result.Company.Purpose.FinancialPurpose.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Purpose.FinancialPurpose.Latest.UpdatedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenIndustriesTest()
    {
        const string CVR_NUMMER = "12626509";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("Kreditmarked og forsikring (65)", response.Result.Company.Industry.Notes);

        Assert.IsNotNull(response.Result.Company.Industry.Current);
        Assert.AreEqual("641900", response.Result.Company.Industry.Current.Code);
        Assert.AreEqual("Andre pengeinstitutters aktiviteter", response.Result.Company.Industry.Current.Description);
        Assert.AreEqual(new DateOnly(2025, 1, 1), response.Result.Company.Industry.Current.Period.From);
        Assert.IsNull(response.Result.Company.Industry.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Industry.Current.UpdatedAt);

        Assert.IsNotNull(response.Result.Company.Industry.Latest);
        Assert.AreEqual("641900", response.Result.Company.Industry.Latest.Code);
        Assert.AreEqual("Andre pengeinstitutters aktiviteter", response.Result.Company.Industry.Latest.Description);
        Assert.AreEqual(new DateOnly(2025, 1, 1), response.Result.Company.Industry.Latest.Period.From);
        Assert.IsNull(response.Result.Company.Industry.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Industry.Latest.UpdatedAt);

        Assert.AreEqual(2, response.Result.Company.Industry.HistoricIndustries.Count());
        Assert.AreEqual(1, response.Result.Company.Industry.SecondaryIndustries.Count());
        Assert.AreEqual(4, response.Result.Company.Industry.HistoricSecondaryIndustries.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenIndustriesAndInactiveTest()
    {
        const string CVR_NUMMER = "12718500";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Industry.Notes);
        Assert.IsNull(response.Result.Company.Industry.Current);

        Assert.IsNotNull(response.Result.Company.Industry.Latest);
        Assert.AreEqual("649900", response.Result.Company.Industry.Latest.Code);
        Assert.AreEqual("Anden finansiel formidling undtagen forsikring og pensionsforsikring, i.a.n.", response.Result.Company.Industry.Latest.Description);
        Assert.AreEqual(new DateOnly(2008, 1, 1), response.Result.Company.Industry.Latest.Period.From);
        Assert.AreEqual(new DateOnly(2015, 6, 25), response.Result.Company.Industry.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Industry.Latest.UpdatedAt);

        Assert.AreEqual(2, response.Result.Company.Industry.HistoricIndustries.Count());
        Assert.AreEqual(0, response.Result.Company.Industry.SecondaryIndustries.Count());
        Assert.AreEqual(0, response.Result.Company.Industry.HistoricSecondaryIndustries.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Type.Current);
        Assert.AreEqual("APS", response.Result.Company.Type.Current.Abbreviation);
        Assert.AreEqual("Anpartsselskab", response.Result.Company.Type.Current.Description);
        Assert.AreEqual(new DateOnly(1989, 11, 1), response.Result.Company.Type.Current.Period.From);
        Assert.IsNull(response.Result.Company.Type.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Type.Current.UpdatedAt);

        Assert.IsNotNull(response.Result.Company.Type.Latest);
        Assert.AreEqual("APS", response.Result.Company.Type.Latest.Abbreviation);
        Assert.AreEqual("Anpartsselskab", response.Result.Company.Type.Latest.Description);
        Assert.AreEqual(new DateOnly(1989, 11, 1), response.Result.Company.Type.Latest.Period.From);
        Assert.IsNull(response.Result.Company.Type.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Type.Latest.UpdatedAt);

        Assert.AreEqual(0, response.Result.Company.Type.HistoricTypes.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndInactiveTest()
    {
        const string CVR_NUMMER = "38696769";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Type.Current);

        Assert.IsNotNull(response.Result.Company.Type.Latest);
        Assert.AreEqual("APS", response.Result.Company.Type.Latest.Abbreviation);
        Assert.AreEqual("Anpartsselskab", response.Result.Company.Type.Latest.Description);
        Assert.AreEqual(new DateOnly(2021, 6, 20), response.Result.Company.Type.Latest.Period.From);
        Assert.AreEqual(new DateOnly(2023, 4, 21), response.Result.Company.Type.Latest.Period.To);
        Assert.IsNotNull(response.Result.Company.Type.Latest.UpdatedAt);

        Assert.AreEqual(2, response.Result.Company.Type.HistoricTypes.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsIsGovernmentalTest()
    {
        const string CVR_NUMMER = "12473192";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Result.Company.Type.IsGovernmental);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsIsGovernmentalAndInactiveTest()
    {
        const string CVR_NUMMER = "10051460";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsTrue(response.Result.Company.Type.IsGovernmental);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsIsGovernmentalAndInactiveAndDatesMismatchTest()
    {
        const string CVR_NUMMER = "12070802";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Type.IsGovernmental);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsPubliclyListedTest()
    {
        const string CVR_NUMMER = "61126228";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Type.IsPubliclyListed);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsPubliclyListedAndInactiveTest()
    {
        const string CVR_NUMMER = "24257967";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsFalse(response.Result.Company.Type.IsPubliclyListed);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsSocialEconomicTest()
    {
        const string CVR_NUMMER = "31858453";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Result.Company.Type.IsSocialEconomic);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsSocialEconomicAndInactiveTest()
    {
        const string CVR_NUMMER = "17195549";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsTrue(response.Result.Company.Type.IsSocialEconomic);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsSocialEconomicAndInactiveAndDatesMismatchTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsCertifiedAuditorTest()
    {
        const string CVR_NUMMER = "10623685";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Type.IsCertifiedAuditor);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsCertifiedAuditorAndInactiveTest()
    {
        const string CVR_NUMMER = "34354170";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsTrue(response.Result.Company.Type.IsCertifiedAuditor);
    }

    [TestMethod]
    public async Task GetCompanyWhenBusinessTypesAndIsCertifiedAuditorAndInactiveAndDatesMismatchTest()
    {
        const string CVR_NUMMER = "10230101";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Type.IsCertifiedAuditor);
    }

    [TestMethod]
    public async Task GetCompanyWhenStatusTest()
    {
        const string CVR_NUMMER = "12626509";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Status.Current);
        Assert.AreEqual("Normal", response.Result.Company.Status.Current.Value);
        Assert.AreEqual(new DateOnly(1986, 1, 2), response.Result.Company.Status.Current.Period.From);
        Assert.IsNull(response.Result.Company.Status.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Status.Current.UpdatedAt);

        Assert.AreEqual(0, response.Result.Company.Status.HistoricStatuses.Count());

        Assert.IsTrue(response.Result.Company.Status.IsActive);
    }

    [TestMethod]
    public async Task GetCompanyWhenStatusAndInactiveTest()
    {
        const string CVR_NUMMER = "34706409";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNotNull(response.Result.Company.Status.Current);
        Assert.AreEqual("Tvangsopløst", response.Result.Company.Status.Current.Value);
        Assert.AreEqual(new DateOnly(2013, 9, 30), response.Result.Company.Status.Current.Period.From);
        Assert.AreEqual(new DateOnly(2013, 9, 30), response.Result.Company.Status.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Status.Current.UpdatedAt);

        Assert.AreEqual(3, response.Result.Company.Status.HistoricStatuses.Count());

        Assert.IsFalse(response.Result.Company.Status.IsActive);
    }

    [TestMethod]
    public async Task GetCompanyWhenStatusAndCreditStatusTest()
    {
        const string CVR_NUMMER = "14357890";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Status.CreditStatus.Current);
        Assert.AreEqual("1", response.Result.Company.Status.CreditStatus.Current.Code);
        Assert.AreEqual("Konkurs", response.Result.Company.Status.CreditStatus.Current.Text);
        Assert.AreEqual("Ophævelse af dekret", response.Result.Company.Status.CreditStatus.Current.Notes);
        Assert.AreEqual(new DateOnly(2009, 6, 9), response.Result.Company.Status.CreditStatus.Current.Period.From);
        Assert.IsNull(response.Result.Company.Status.CreditStatus.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Status.CreditStatus.Current.UpdatedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenStatusAndCreditStatusAndInactiveTest()
    {
        const string CVR_NUMMER = "14187340";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNotNull(response.Result.Company.Status.CreditStatus.Current);
        Assert.AreEqual("3", response.Result.Company.Status.CreditStatus.Current.Code);
        Assert.AreEqual("Tvangsakkord", response.Result.Company.Status.CreditStatus.Current.Text);
        Assert.AreEqual("Regnskab og boafslutning", response.Result.Company.Status.CreditStatus.Current.Notes);
        Assert.AreEqual(new DateOnly(2017, 11, 10), response.Result.Company.Status.CreditStatus.Current.Period.From);
        Assert.IsNull(response.Result.Company.Status.CreditStatus.Current.Period.To);
        Assert.IsNotNull(response.Result.Company.Status.CreditStatus.Current.UpdatedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenEmployementTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Employment.Employees.Current.NumberOfEmployees > 400);
        Assert.IsTrue(response.Result.Company.Employment.Employees.Current.NumberOfEmployees < 600);
        Assert.IsTrue(response.Result.Company.Employment.Employees.Current.NumberOfFulltimePositions > 400);
        Assert.IsTrue(response.Result.Company.Employment.Employees.Current.NumberOfFulltimePositions < 600);
        Assert.IsNotNull(response.Result.Company.Employment.Employees.Current.UpdatedAt);
        Assert.IsTrue(response.Result.Company.Employment.Employees.HistoricEmployees.Count() > 182);
    }

    [TestMethod]
    public async Task GetCompanyWhenEmploymentAndInactiveTest()
    {
        const string CVR_NUMMER = "38696769";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.Employment.Employees.Current);
        Assert.AreEqual(6, response.Result.Company.Employment.Employees.HistoricEmployees.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenManagersTest()
    {
        const string CVR_NUMMER = "12861303";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Employment.Managers.Current);
        Assert.AreEqual(1, response.Result.Company.Employment.Managers.Current.Count());
        Assert.AreEqual(0, response.Result.Company.Employment.Managers.HistoricManagers.Count());

        var manager = response.Result.Company.Employment.Managers.Current.FirstOrDefault();
        Assert.IsNotNull(manager);
        Assert.AreEqual("Jørgen Kent Jensen", manager.Name);
        Assert.AreEqual("Daglig ledelse", manager.Title);
        Assert.IsNull(manager.ElectionMethod);
    }

    [TestMethod]
    public async Task GetCompanyWhenRegisteredCapitalTest()
    {
        const string CVR_NUMMER = "30276582";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.RegisteredCapital.Current.Value);
        Assert.AreEqual("DKK", response.Result.Company.RegisteredCapital.Current.Currency);

        Assert.AreEqual(26, response.Result.Company.RegisteredCapital.HistoricRegisteredCapitals.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenRegisteredCapitalAndInactiveTest()
    {
        const string CVR_NUMMER = "38696769";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.RegisteredCapital.Current);
        Assert.AreEqual(2, response.Result.Company.RegisteredCapital.HistoricRegisteredCapitals.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenRegisteredCapitalAndPartiallyPaidTest()
    {
        const string CVR_NUMMER = "34688362";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.RegisteredCapital.IsPartiallyPaid);
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialYearTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.FinancialYear.Current);
        Assert.AreEqual(7, response.Result.Company.FinancialYear.Current.Begin.Month);
        Assert.AreEqual(1, response.Result.Company.FinancialYear.Current.Begin.Day);
        Assert.AreEqual(6, response.Result.Company.FinancialYear.Current.End.Month);
        Assert.AreEqual(30, response.Result.Company.FinancialYear.Current.End.Day);

        Assert.IsNotNull(response.Result.Company.FinancialYear.Latest);
        Assert.AreEqual(7, response.Result.Company.FinancialYear.Latest.Begin.Month);
        Assert.AreEqual(1, response.Result.Company.FinancialYear.Latest.Begin.Day);
        Assert.AreEqual(6, response.Result.Company.FinancialYear.Latest.End.Month);
        Assert.AreEqual(30, response.Result.Company.FinancialYear.Latest.End.Day);

        Assert.AreEqual(new DateOnly(1989, 11, 1), response.Result.Company.FinancialYear.FirstFinancialYear.From);
        Assert.AreEqual(new DateOnly(1990, 6, 30), response.Result.Company.FinancialYear.FirstFinancialYear.To);

        Assert.AreEqual(0, response.Result.Company.FinancialYear.HistoricFinancialYears.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialYearAndInactiveTest()
    {
        const string CVR_NUMMER = "34354170";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.FinancialYear.Current);

        Assert.IsNotNull(response.Result.Company.FinancialYear.Latest);
        Assert.AreEqual(1, response.Result.Company.FinancialYear.Latest.Begin.Month);
        Assert.AreEqual(1, response.Result.Company.FinancialYear.Latest.Begin.Day);
        Assert.AreEqual(12, response.Result.Company.FinancialYear.Latest.End.Month);
        Assert.AreEqual(31, response.Result.Company.FinancialYear.Latest.End.Day);

        Assert.AreEqual(new DateOnly(2011, 10, 1), response.Result.Company.FinancialYear.FirstFinancialYear.From);
        Assert.AreEqual(new DateOnly(2012, 12, 31), response.Result.Company.FinancialYear.FirstFinancialYear.To);

        Assert.AreEqual(1, response.Result.Company.FinancialYear.HistoricFinancialYears.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialYearAndOngoingTransitionPeriodTest()
    {
        const string CVR_NUMMER = "14531688";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.FinancialYear.OngoingTransitionPeriod);
        Assert.AreEqual(new DateOnly(1996, 5, 1), response.Result.Company.FinancialYear.OngoingTransitionPeriod.From);
        Assert.AreEqual(new DateOnly(1996, 6, 30), response.Result.Company.FinancialYear.OngoingTransitionPeriod.To);
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialYearAndOngoingTransitionPeriodAndInactiveTest()
    {
        const string CVR_NUMMER = "14511490";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsNull(response.Result.Company.FinancialYear.OngoingTransitionPeriod);
    }

    [TestMethod]
    public async Task GetCompanyWhenFinancialYearAndNotesTest()
    {
        const string CVR_NUMMER = "25559177";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("søndag efter den lørdag der ligger nærmest den sidste dag i marts til den lørdag der ligger nærmest den sidste dag i marts det efterfølgende år.", response.Result.Company.FinancialYear.Notes);
    }

    [TestMethod]
    public async Task GetCompanyWhenAntiMoneyLaunderingTest()
    {
        const string CVR_NUMMER = "14194711";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("Registret til bekæmpelse af hvidvask", response.Result.Company.AntiMoneyLaundering.Text);
    }

    [TestMethod]
    public async Task GetCompanyWhenAntiMoneyLaunderingAndInactiveTest()
    {
        const string CVR_NUMMER = "17842579";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.AreEqual("Registret til bekæmpelse af hvidvask", response.Result.Company.AntiMoneyLaundering.Text);
        Assert.AreEqual(1, response.Result.Company.AntiMoneyLaundering.Activities.Count());
        Assert.AreEqual("Selskabsfabrikant", response.Result.Company.AntiMoneyLaundering.Activities.FirstOrDefault());
    }

    [TestMethod]
    public async Task GetCompanyWhenAntiMoneyLaunderingAndIsSubjectedToLawAboutMoneyLaunderingAndTerrorFinancingTest()
    {
        const string CVR_NUMMER = "14194711";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.AntiMoneyLaundering.IsSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing);
    }

    [TestMethod]
    public async Task GetCompanyWhenAntiMoneyLaunderingAndActivitiesTest()
    {
        const string CVR_NUMMER = "14149341";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(1, response.Result.Company.AntiMoneyLaundering.Activities.Count());
        Assert.AreEqual("Selskabsfabrikant", response.Result.Company.AntiMoneyLaundering.Activities.FirstOrDefault());
    }

    [TestMethod]
    public async Task GetCompanyWhenAntiMoneyLaunderingAndAntiMoneyLaunderingAppointeeTest()
    {
        const string CVR_NUMMER = "14149341";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.AntiMoneyLaundering.AntiMoneyLaunderingAppointees.Current);
        Assert.AreEqual("Leder, Reel ejer", response.Result.Company.AntiMoneyLaundering.AntiMoneyLaunderingAppointees.Current.Title);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasAuditTest()
    {
        const string CVR_NUMMER = "17650483";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Auditing.HasAudit);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasAuditAndInactiveTest()
    {
        const string CVR_NUMMER = "17668382";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsTrue(response.Result.Company.Auditing.HasAudit);
    }

    [TestMethod]
    public async Task GetCompanyWhenAuditorTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Auditing.Auditor);
        Assert.AreEqual("DELOITTE STATSAUTORISERET REVISIONSPARTNERSELSKAB", response.Result.Company.Auditing.Auditor.Current.Name);
        Assert.AreEqual("5651072", response.Result.Company.Auditing.Auditor.Current.ExternalId);
        Assert.AreEqual("Weidekampsgade", response.Result.Company.Auditing.Auditor.Current.Address.StreetName);
        Assert.AreEqual("København S", response.Result.Company.Auditing.Auditor.Current.Address.City.Name);
        Assert.AreEqual(EntityType.Company, response.Result.Company.Auditing.Auditor.Current.EntityType);
        Assert.AreEqual("33963556", response.Result.Company.Auditing.Auditor.Current.RegistrationNumber);

        Assert.AreEqual(3, response.Result.Company.Auditing.Auditor.HistoricAuditors.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenSustainabilityAuditorTest()
    {
        const string CVR_NUMMER = "64806815";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Auditing.SustainabilityAuditor.Current);
        Assert.AreEqual("PRICEWATERHOUSECOOPERS STATSAUTORISERET REVISIONSPARTNERSELSKAB", response.Result.Company.Auditing.SustainabilityAuditor.Current.Name);
        Assert.AreEqual("4000769917", response.Result.Company.Auditing.SustainabilityAuditor.Current.ExternalId);
        Assert.AreEqual("Strandvejen", response.Result.Company.Auditing.SustainabilityAuditor.Current.Address.StreetName);
        Assert.AreEqual("Hellerup", response.Result.Company.Auditing.SustainabilityAuditor.Current.Address.City.Name);
        Assert.AreEqual(EntityType.Company, response.Result.Company.Auditing.SustainabilityAuditor.Current.EntityType);
        Assert.AreEqual("33771231", response.Result.Company.Auditing.SustainabilityAuditor.Current.RegistrationNumber);

        Assert.AreEqual(0, response.Result.Company.Auditing.SustainabilityAuditor.HistoricAuditors.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenIsOvertakenByFinansialStabilityAuthorityTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //const string CVR_NUMMER = "";

        //var response = await this.DanishCvrService
        //    .GetCompanyAsync(CVR_NUMMER);

        //await this.SaveJsonOrDefault(response.Result);

        //Assert.IsNotNull(response);
        //Assert.IsNotNull(response.Result);

        //Assert.IsTrue(response.Result.Company.Auditing.IsOvertakenByFinancialStabilityAuthority);
    }

    [TestMethod]
    public async Task GetCompanyWhenSignatoryRuleTest()
    {
        const string CVR_NUMMER = "30276582";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("Selskabet tegnes af tre bestyrelsesmedlemmer i forening eller af selskabets administrerende direktør i forening med én direktør eller ét bestyrelsesmedlem", response.Result.Company.Authority.SignatoryRule);
    }

    [TestMethod]
    public async Task GetCompanyWhenSignatoryRuleAndInactiveTest()
    {
        const string CVR_NUMMER = "38696769";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.AreEqual("Selskabet tegnes af direktionen alene.", response.Result.Company.Authority.SignatoryRule);
    }

    [TestMethod]
    public async Task GetCompanyWhenExecutivesTest()
    {
        const string CVR_NUMMER = "30276582";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(1, response.Result.Company.Authority.Executives.Current.Count());
        Assert.AreEqual(4, response.Result.Company.Authority.Executives.HistoricExecutives.Count());

        var executive = response.Result.Company.Authority.Executives.HistoricExecutives.FirstOrDefault(x => x.Title != null);
        Assert.IsNotNull(executive);
        Assert.AreEqual("Direktør", executive.Title);
    }

    [TestMethod]
    public async Task GetCompanyWhenAuthorizedSignatoriesTest()
    {
        const string CVR_NUMMER = "42583065";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.AuthorizedSignatories.Current);
        Assert.AreEqual(1, response.Result.Company.Authority.AuthorizedSignatories.Current.Count());
        Assert.AreEqual(0, response.Result.Company.Authority.AuthorizedSignatories.HistoricAuthorizedSignatories.Count());

        var authorizedSignatory = response.Result.Company.Authority.AuthorizedSignatories.Current.FirstOrDefault();
        Assert.IsNotNull(authorizedSignatory);
        Assert.AreEqual("VIA EQUITY A/S", authorizedSignatory.Name);
    }

    [TestMethod]
    public async Task GetCompanyWhenAuthorizedSignatoriesAndNoCurrentTest()
    {
        const string CVR_NUMMER = "62447559";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.AuthorizedSignatories.Current);
        Assert.AreEqual(0, response.Result.Company.Authority.AuthorizedSignatories.Current.Count());
        Assert.AreEqual(4, response.Result.Company.Authority.AuthorizedSignatories.HistoricAuthorizedSignatories.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenSpecialFinancialParticipantsTest()
    {
        const string CVR_NUMMER = "14211349";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.SpecialFinancialParticipants.Current);
        Assert.AreEqual(2, response.Result.Company.Authority.SpecialFinancialParticipants.Current.Count());
        Assert.AreEqual(5, response.Result.Company.Authority.SpecialFinancialParticipants.HistoricSpecialFinancialParticipants.Count());

        var financialParticipant = response.Result.Company.Authority.SpecialFinancialParticipants.Current.FirstOrDefault();
        Assert.IsNotNull(financialParticipant);
        Assert.AreEqual("C WorldWide Fund Management, filial af C WorldWide Fund Management S.A., Luxembourg", financialParticipant.Name);
        Assert.AreEqual("Administrationsselskab", financialParticipant.Type);
    }

    [TestMethod]
    public async Task GetCompanyWhenSpecialFinancialParticipantsAndNoCurrentTest()
    {
        const string CVR_NUMMER = "14090746";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.SpecialFinancialParticipants.Current);
        Assert.AreEqual(0, response.Result.Company.Authority.SpecialFinancialParticipants.Current.Count());
        Assert.AreEqual(3, response.Result.Company.Authority.SpecialFinancialParticipants.HistoricSpecialFinancialParticipants.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenOtherLiableParticipantsTest()
    {
        const string CVR_NUMMER = "14068341";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.OtherLiableParticipants.Current);
        Assert.AreEqual(1, response.Result.Company.Authority.OtherLiableParticipants.Current.Count());
        Assert.AreEqual(0, response.Result.Company.Authority.OtherLiableParticipants.HistoricLiableParticipants.Count());

        var liableParticipant = response.Result.Company.Authority.OtherLiableParticipants.Current.FirstOrDefault();
        Assert.IsNotNull(liableParticipant);
        Assert.AreEqual("Michael Jensen", liableParticipant.Name);
        Assert.AreEqual("Interessenter", liableParticipant.Role);
        Assert.IsNull(liableParticipant.RegisteredCapital);
    }

    [TestMethod]
    public async Task GetCompanyWhenOtherLiableParticipantsAndRegisteredCapitalTest()
    {
        const string CVR_NUMMER = "32079873";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var liableParticipant = response.Result.Company.Authority.OtherLiableParticipants.Current.FirstOrDefault();
        Assert.IsNotNull(liableParticipant);
        Assert.IsNotNull(liableParticipant.RegisteredCapital);
        Assert.AreEqual(0.00D, liableParticipant.RegisteredCapital.Value);
        Assert.AreEqual("DKK", liableParticipant.RegisteredCapital.Currency);
    }

    [TestMethod]
    public async Task GetCompanyWhenOtherLiableParticipantsAndNoCurrentTest()
    {
        const string CVR_NUMMER = "14026045";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.OtherLiableParticipants.Current);
        Assert.AreEqual(0, response.Result.Company.Authority.OtherLiableParticipants.Current.Count());
        Assert.AreEqual(1, response.Result.Company.Authority.OtherLiableParticipants.HistoricLiableParticipants.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenLiquidatorTest()
    {
        const string CVR_NUMMER = "10190045";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.Liquidators);
        Assert.AreEqual(1, response.Result.Company.Authority.Liquidators.Count());

        var liquidator = response.Result.Company.Authority.Liquidators.FirstOrDefault();
        Assert.IsNotNull(liquidator);
        Assert.AreEqual("Hans Lindstrøm Svendsen", liquidator.Name);
        Assert.AreEqual("Skifteretten", liquidator.AppointedBy);
    }

    [TestMethod]
    public async Task GetCompanyWhenLiquidatorAndMultipleTest()
    {
        const string CVR_NUMMER = "10195004";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Authority.Liquidators);
        Assert.AreEqual(3, response.Result.Company.Authority.Liquidators.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenOversightAuthorityTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //const string CVR_NUMMER = "69486312";

        //var response = await this.DanishCvrService
        //    .GetCompanyAsync(CVR_NUMMER);

        //await this.SaveJsonOrDefault(response.Result);

        //Assert.IsNotNull(response);
        //Assert.IsNotNull(response.Result);

        //Assert.AreEqual("Erhvervsstyrelse, Civilstyrelsen", response.Result.Company.Governance.Oversight.Authority);
    }

    [TestMethod]
    public async Task GetCompanyWhenOversightAuthorityWhenDefaultTest()
    {
        const string CVR_NUMMER = "27157351";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("Erhvervsstyrelsen", response.Result.Company.Governance.Oversight.Authority);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasSocialEconomicOversightTest()
    {
        const string CVR_NUMMER = "25940539";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Governance.Oversight.HasSocialEconomicOversight);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasSocialEconomicOversightAndInactiveTest()
    {
        const string CVR_NUMMER = "25853709";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.Company.Status.IsActive);
        Assert.IsFalse(response.Result.Company.Governance.Oversight.HasSocialEconomicOversight);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasSpecialLicensesOrConcessionsTest()
    {
        const string CVR_NUMMER = "10496837";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Governance.Oversight.HasSpecialLicensesOrConcessions);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasSpecialLicensesOrConcessionsAndExpiredTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetCompanyWhenNotesTest()
    {
        const string CVR_NUMMER = "27157351";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("4 - erst rykker for årsrapport", response.Result.Company.Governance.Oversight.Notes);
    }

    [TestMethod]
    public async Task GetCompanyWhenNotesSocialEconomicTest()
    {
        const string CVR_NUMMER = "55280312";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.AreEqual("Socialtilsyn Hovedstaden", response.Result.Company.Governance.Oversight.Notes);
    }

    [TestMethod]
    public async Task GetCompanyWhenNotesAndNotesSocialEconomicTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetCompanyWhenBoardMembersTest()
    {
        const string CVR_NUMMER = "10192676";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Governance.BoardMembers.Current);
        Assert.AreEqual(9, response.Result.Company.Governance.BoardMembers.Current.Count());
        Assert.AreEqual(1, response.Result.Company.Governance.BoardMembers.HistoricBoardMembers.Count());

        var boardMember = response.Result.Company.Governance.BoardMembers.Current.FirstOrDefault();
        Assert.IsNotNull(boardMember);
        Assert.AreEqual("Erik Gregers Hansen", boardMember.Name);
        Assert.AreEqual("Bestyrelsesmedlem", boardMember.Title);
        Assert.IsNull(boardMember.AlternateFor);
        Assert.IsNull(boardMember.ElectionMethod);
    }

    [TestMethod]
    public async Task GetCompanyWhenBoardMembersAndDirective8ApprovedTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetCompanyWhenAssociationRepresentativesTest()
    {
        const string CVR_NUMMER = "10194105";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Governance.AssociationRepresentatives.Current);
        Assert.AreEqual(1, response.Result.Company.Governance.AssociationRepresentatives.Current.Count());
        Assert.AreEqual(0, response.Result.Company.Governance.AssociationRepresentatives.HistoricAssociationRepresentatives.Count());

        var associationRepresentative = response.Result.Company.Governance.AssociationRepresentatives.Current.FirstOrDefault();
        Assert.IsNotNull(associationRepresentative);
        Assert.AreEqual("Judith Søndergaard", associationRepresentative.Name);
        Assert.AreEqual("Foreningsrepræsentant", associationRepresentative.Title);
    }

    [TestMethod]
    public async Task GetCompanyWhenFoundersTest()
    {
        const string CVR_NUMMER = "43804146";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Governance.Founders);
        Assert.AreEqual(6, response.Result.Company.Governance.Founders.Count());

        var founder = response.Result.Company.Governance.Founders.FirstOrDefault();
        Assert.IsNotNull(founder);
        Assert.AreEqual("ROCKET ApS", founder.Name);
        Assert.IsNotNull(founder.RegistrationNumber);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasShareClassesTest()
    {
        const string CVR_NUMMER = "12932502";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Ownership.HasShareClasses);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasOnlyUnder5PercentOwnershipsTest()
    {
        const string CVR_NUMMER = "42914126";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Ownership.HasOnlyUnder5PercentOwnerships);
    }

    [TestMethod]
    public async Task GetCompanyWhenHasPublicShareholderRegistryTest()
    {
        const string CVR_NUMMER = "22166514";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Ownership.HasPublicShareholderRegistry);
    }

    [TestMethod]
    public async Task GetCompanyWhenLegalOwnersTest()
    {
        const string CVR_NUMMER = "43804146";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Ownership.LegalOwners);
        Assert.AreEqual(8, response.Result.Company.Ownership.LegalOwners.Count());

        var legalOwner = response.Result.Company.Ownership.LegalOwners.FirstOrDefault();
        Assert.IsNotNull(legalOwner);
        Assert.AreEqual("ROCKET ApS", legalOwner.Name);
        Assert.IsNotNull(legalOwner.RegistrationNumber);
        Assert.IsNotNull(legalOwner.Equity.Current);
        Assert.IsNull(legalOwner.Equity.Current.ShareClass);
        Assert.AreEqual(0.3333, legalOwner.Equity.Current.SharePercentage);
        Assert.IsNotNull(legalOwner.VotingRights.Current);
        Assert.AreEqual(0.3333, legalOwner.VotingRights.Current.Value);

        Assert.AreEqual(2, legalOwner.Equity.HistoricValues.Count());
        Assert.AreEqual(2, legalOwner.VotingRights.HistoricValues.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenBeneficialOwnersTest()
    {
        const string CVR_NUMMER = "43804146";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Ownership.BeneficialOwners);
        Assert.AreEqual(4, response.Result.Company.Ownership.BeneficialOwners.Count());

        var beneficialOwner = response.Result.Company.Ownership.BeneficialOwners.FirstOrDefault();
        Assert.IsNotNull(beneficialOwner);
        Assert.AreEqual("Simon Bækgaard Kristoffersen", beneficialOwner.Name);
        Assert.IsNotNull(beneficialOwner.Equity.Current);
        Assert.IsNull(beneficialOwner.Equity.Current.ShareClass);
        Assert.AreEqual(0.45, beneficialOwner.Equity.Current.SharePercentage);
        Assert.IsNotNull(beneficialOwner.VotingRights.Current);
        Assert.AreEqual(0.45, beneficialOwner.VotingRights.Current.Value);

        Assert.AreEqual(4, beneficialOwner.Equity.HistoricValues.Count());
        Assert.AreEqual(4, beneficialOwner.VotingRights.HistoricValues.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenBeneficiaryTest()
    {
        const string CVR_NUMMER = "71981312";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Ownership.Beneficiary);
        Assert.AreEqual("Tidligere koncerndirektører i jyske bank a/s samt disses efterladte ægtefæller og børn.", response.Result.Company.Ownership.Beneficiary.Text);
        Assert.AreEqual("Fonden har til formål at yde pensionstilskud til medlemmer af koncerndirektionen i jyske bank a/s samt disses eventuelle enker eller enkemænd og mindreårige børn samt børn under 24 år under uddannelse, i det omfang fondens midler tillader dette.", response.Result.Company.Ownership.Beneficiary.LegalEntitlement);
    }

    [TestMethod]
    public async Task GetCompanyWhenBeneficiaryAndInactiveTest()
    {
        const string CVR_NUMMER = "12418434";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNull(response.Result.Company.Ownership.Beneficiary);
    }

    [TestMethod]
    public async Task GetCompanyWhenParentCompanyTest()
    {
        const string CVR_NUMMER = "12031041";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Ownership.ParentCompany.Current);
        Assert.AreEqual("4005870900", response.Result.Company.Ownership.ParentCompany.Current.ExternalId);
        Assert.AreEqual(1, response.Result.Company.Ownership.ParentCompany.HistoricParentCompanies.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenMergerTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetCompanyWhenDemergerTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetCompanyWhenBilawsTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Bilaws.Current);
        Assert.IsNull(response.Result.Company.Bilaws.Current.Notes);
        Assert.IsNull(response.Result.Company.Bilaws.Approval);
        Assert.AreEqual(1, response.Result.Company.Bilaws.HistoricBilaws.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenBilawsAndApprovalTest()
    {
        const string CVR_NUMMER = "32290442";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.Bilaws.Current);
        Assert.IsNotNull(response.Result.Company.Bilaws.Approval);
        Assert.AreEqual("Finanstilsynet", response.Result.Company.Bilaws.Approval.Authority);
        Assert.IsNotNull(response.Result.Company.Bilaws.Approval.ApprovedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenAuditorRegistrationTest()
    {
        const string CVR_NUMMER = "31882354";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.AuditorRegistration);
        Assert.IsTrue(response.Result.Company.AuditorRegistration.IsHoldingCompany);
        Assert.AreEqual("Boersnoteret eu", response.Result.Company.AuditorRegistration.PublicInterest);
        Assert.AreEqual("Anders Dons", response.Result.Company.AuditorRegistration.ContactPerson);
        Assert.AreEqual("Deloitte touche tohmatsu limited\r\nhjemmeside: http://deloitte.com", response.Result.Company.AuditorRegistration.ProfessionalNetwork);
        Assert.IsNotNull(response.Result.Company.AuditorRegistration.CertifiedAuditors.Current);
        Assert.IsTrue(response.Result.Company.AuditorRegistration.CertifiedAuditors.Current.Count() > 100);
        Assert.AreEqual(0, response.Result.Company.AuditorRegistration.CertifiedAuditors.HistoricCertifiedAuditors.Count());

        var certifiedAuditor = response.Result.Company.AuditorRegistration.CertifiedAuditors.Current.FirstOrDefault();
        Assert.IsNotNull(certifiedAuditor);
        Assert.AreEqual("Lars Berg-Nielsen", certifiedAuditor.Name);
        Assert.AreEqual("Revisionsvirksomhed Stemmeberettiget", certifiedAuditor.Role);
        Assert.AreEqual("Weidekampsgade 6, 2300 københavn s", certifiedAuditor.BusinessAddress);
        Assert.AreEqual(0.00, certifiedAuditor.VotingRights.Current.Value);
        Assert.AreEqual("Anden", certifiedAuditor.VotingRights.Type);
        Assert.IsNull(certifiedAuditor.VotingRights.Exception);
    }

    [TestMethod]
    public async Task GetCompanyWhenAuditorRegistrationAndVotingRightsTest()
    {
        const string CVR_NUMMER = "33864043";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.AuditorRegistration);

        var certifiedAuditor = response.Result.Company.AuditorRegistration.CertifiedAuditors.Current.FirstOrDefault();
        Assert.IsNotNull(certifiedAuditor);
        Assert.AreEqual(0.00, certifiedAuditor.VotingRights.Current.Value);
        Assert.IsNull(certifiedAuditor.VotingRights.Type);
        Assert.AreEqual("Hovedbeskaeftigelse", certifiedAuditor.VotingRights.Exception);
    }

    [TestMethod]
    public async Task GetCompanyWhenAuditorRegistrationAndInActiveTest()
    {
        const string CVR_NUMMER = "21158178";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNull(response.Result.Company.AuditorRegistration);
    }

    [TestMethod]
    public async Task GetCompanyWhenProductionUnitsTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(1, response.Result.Company.ProductionUnits.Current.Count());
        Assert.AreEqual(1, response.Result.Company.ProductionUnits.HistoricProductionUnits.Count());

        var productionUnitId = response.Result.Company.ProductionUnits.Current.FirstOrDefault();
        Assert.IsNotNull(productionUnitId);
        Assert.AreEqual("1000594476", productionUnitId.ProductionUnitNumber);
    }

    [TestMethod]
    public async Task GetCompanyWhenProductionUnitsAndInactiveTest()
    {
        const string CVR_NUMMER = "10033780";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(0, response.Result.Company.ProductionUnits.Current.Count());
        Assert.AreEqual(1, response.Result.Company.ProductionUnits.HistoricProductionUnits.Count());
    }

    [TestMethod]
    public async Task GetCompanyWhenFoundedAtTest()
    {
        const string CVR_NUMMER = "10190029";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.FoundedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenEffectiveStartedAtTest()
    {
        const string CVR_NUMMER = "10190029";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.EffectiveStartedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenDissolvedAtTest()
    {
        const string CVR_NUMMER = "10190029";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.DissolvedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenCommercialFundApprovedAtTest()
    {
        const string CVR_NUMMER = "10192676";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.Company.CommercialFundApprovedAt);
    }

    [TestMethod]
    public async Task GetCompanyWhenIsProtectedFromAdvertisementTest()
    {
        const string CVR_NUMMER = "10192676";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.IsProtectedFromAdvertisement);
    }

    [TestMethod]
    public async Task GetCompanyWhenEntityTypeTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(EntityType.Company, response.Result.Company.EntityType);
    }

    [TestMethod]
    public async Task GetCompanyWhenImportErrorsTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsTrue(response.Result.Company.Errors.HasImportErrors);
        Assert.AreEqual("Medlem: Ukendt deltager enhedsnummer for cvr: 13612870 4004428861", response.Result.Company.Errors.Description);
    }

    [TestMethod]
    public async Task GetCompanyWhenRegistrationErrorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //const string CVR_NUMMER = "";

        //var response = await this.DanishCvrService
        //    .GetCompanyAsync(CVR_NUMMER);

        //await this.SaveJsonOrDefault(response.Result);

        //Assert.IsNotNull(response);
        //Assert.IsNotNull(response.Result);
        
        //Assert.IsTrue(response.Result.Company.Errors.HasRegistrationErrors);
    }

    [TestMethod]
    public async Task GetCompanyWhenExternalIdTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.IsNotNull(response.Result.Company.ExternalId);
    }

    [TestMethod]
    public async Task GetCompanyWhenUpdatedAtTest()
    {
        const string CVR_NUMMER = "13612870";

        var response = await this.DanishCvrService
            .GetCompanyAsync(CVR_NUMMER);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.IsNotNull(response.Result.Company.UpdatedAt);
    }
}