using System;
using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Responses.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetProductionUnitTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenNamesTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.ProductionUnit.Names.Current);
        Assert.AreEqual("MICROSOFT DANMARK APS", response.Result.ProductionUnit.Names.Current.Value);
        Assert.AreEqual(new DateOnly(1989, 11, 1), response.Result.ProductionUnit.Names.Current.Period.From);
        Assert.IsNull(response.Result.ProductionUnit.Names.Current.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.Names.Current.UpdatedAt);

        Assert.IsNotNull(response.Result.ProductionUnit.Names.Latest);
        Assert.AreEqual("MICROSOFT DANMARK APS", response.Result.ProductionUnit.Names.Latest.Value);
        Assert.AreEqual(new DateOnly(1989, 11, 1), response.Result.ProductionUnit.Names.Latest.Period.From);
        Assert.IsNull(response.Result.ProductionUnit.Names.Latest.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.Names.Latest.UpdatedAt);

        Assert.AreEqual(0, response.Result.ProductionUnit.Names.HistoricNames.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenNamesAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1021080361";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.Names.Current);

        Assert.IsNotNull(response.Result.ProductionUnit.Names.Latest);
        Assert.AreEqual(".dk Invest Holding ivs", response.Result.ProductionUnit.Names.Latest.Value);
        Assert.AreEqual(new DateOnly(2016, 01, 22), response.Result.ProductionUnit.Names.Latest.Period.From);
        Assert.AreEqual(new DateOnly(2017, 11, 17), response.Result.ProductionUnit.Names.Latest.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.Names.Latest.UpdatedAt);

        Assert.AreEqual(2, response.Result.ProductionUnit.Names.HistoricNames.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenAddressesTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.ProductionUnit.Addresses.Current);

        Assert.AreEqual("DK", response.Result.ProductionUnit.Addresses.Current.Country);
        Assert.AreEqual("Kgs. Lyngby", response.Result.ProductionUnit.Addresses.Current.City.Name);
        Assert.AreEqual("2800", response.Result.ProductionUnit.Addresses.Current.City.PostalCode);
        Assert.AreEqual("Kanalvej", response.Result.ProductionUnit.Addresses.Current.StreetName);
        Assert.AreEqual("7", response.Result.ProductionUnit.Addresses.Current.HouseNumberFrom);

        Assert.AreEqual("DK", response.Result.ProductionUnit.Addresses.Latest.Country);
        Assert.AreEqual("Kgs. Lyngby", response.Result.ProductionUnit.Addresses.Latest.City.Name);
        Assert.AreEqual("2800", response.Result.ProductionUnit.Addresses.Latest.City.PostalCode);
        Assert.AreEqual("Kanalvej", response.Result.ProductionUnit.Addresses.Latest.StreetName);
        Assert.AreEqual("7", response.Result.ProductionUnit.Addresses.Latest.HouseNumberFrom);

        Assert.AreEqual(2, response.Result.ProductionUnit.Addresses.HistoricAddresses.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenAddressesAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1021080361";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.Addresses.Current);

        Assert.IsNotNull(response.Result.ProductionUnit.Addresses.Latest);
        Assert.AreEqual("DK", response.Result.ProductionUnit.Addresses.Latest.Country);
        Assert.AreEqual("Greve", response.Result.ProductionUnit.Addresses.Latest.City.Name);
        Assert.AreEqual("2670", response.Result.ProductionUnit.Addresses.Latest.City.PostalCode);
        Assert.AreEqual("Korskildelund", response.Result.ProductionUnit.Addresses.Latest.StreetName);
        Assert.AreEqual("6", response.Result.ProductionUnit.Addresses.Latest.HouseNumberFrom);

        Assert.AreEqual(2, response.Result.ProductionUnit.Addresses.HistoricAddresses.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenPhoneNumbersTest()
    {
        const string PRODUCTION_UNIT_ID = "1011684358";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsNotNull(response.Result.ProductionUnit.PhoneNumbers.Current);
        Assert.AreEqual("45129184", response.Result.ProductionUnit.PhoneNumbers.Current.Value);
        Assert.IsFalse(response.Result.ProductionUnit.PhoneNumbers.Current.IsUnlisted);
        Assert.AreEqual(new DateOnly(2022, 7, 27), response.Result.ProductionUnit.PhoneNumbers.Current.Period.From);
        Assert.IsNull(response.Result.ProductionUnit.PhoneNumbers.Current.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.PhoneNumbers.Current.UpdatedAt);
        Assert.AreEqual(0, response.Result.ProductionUnit.PhoneNumbers.HistoricPhoneNumbers.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenPhoneNumbersAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1021080361";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.PhoneNumbers);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenFaxNumbersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenFaxNumbersAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1021080361";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.FaxNumbers);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenEmailAddressesTest()
    {
        const string PRODUCTION_UNIT_ID = "1011684358";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsNotNull(response.Result.ProductionUnit.EmailAddresses.Current);
        Assert.AreEqual("jobau@danskebank.dk", response.Result.ProductionUnit.EmailAddresses.Current.Value);
        Assert.IsFalse(response.Result.ProductionUnit.EmailAddresses.Current.IsUnlisted);
        Assert.AreEqual(new DateOnly(2022, 7, 27), response.Result.ProductionUnit.EmailAddresses.Current.Period.From);
        Assert.IsNull(response.Result.ProductionUnit.EmailAddresses.Current.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.EmailAddresses.Current.UpdatedAt);
        Assert.AreEqual(0, response.Result.ProductionUnit.EmailAddresses.HistoricEmailAddresses.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenEmailAddressesAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1021080361";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.EmailAddresses.Current);
        Assert.AreEqual(1, response.Result.ProductionUnit.EmailAddresses.HistoricEmailAddresses.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenIndustriesTest()
    {
        const string PRODUCTION_UNIT_ID = "1002927924";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.AreEqual("Kreditmarked og forsikring (65)", response.Result.ProductionUnit.Industry.Notes);

        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Current);
        Assert.AreEqual("641900", response.Result.ProductionUnit.Industry.Current.Code);
        Assert.AreEqual("Andre pengeinstitutters aktiviteter", response.Result.ProductionUnit.Industry.Current.Description);
        Assert.AreEqual(new DateOnly(2025, 1, 1), response.Result.ProductionUnit.Industry.Current.Period.From);
        Assert.IsNull(response.Result.ProductionUnit.Industry.Current.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Current.UpdatedAt);

        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Latest);
        Assert.AreEqual("641900", response.Result.ProductionUnit.Industry.Latest.Code);
        Assert.AreEqual("Andre pengeinstitutters aktiviteter", response.Result.ProductionUnit.Industry.Latest.Description);
        Assert.AreEqual(new DateOnly(2025, 1, 1), response.Result.ProductionUnit.Industry.Latest.Period.From);
        Assert.IsNull(response.Result.ProductionUnit.Industry.Latest.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Latest.UpdatedAt);

        Assert.AreEqual(2, response.Result.ProductionUnit.Industry.HistoricIndustries.Count());
        Assert.AreEqual(0, response.Result.ProductionUnit.Industry.SecondaryIndustries.Count());
        Assert.AreEqual(0, response.Result.ProductionUnit.Industry.HistoricSecondaryIndustries.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenIndustriesAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1007636896";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.Industry.Notes);
        Assert.IsNull(response.Result.ProductionUnit.Industry.Current);

        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Latest);
        Assert.AreEqual("980000", response.Result.ProductionUnit.Industry.Latest.Code);
        Assert.AreEqual("Uoplyst", response.Result.ProductionUnit.Industry.Latest.Description);
        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Latest.Period.From);
        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Latest.Period.To);
        Assert.IsNotNull(response.Result.ProductionUnit.Industry.Latest.UpdatedAt);

        Assert.AreEqual(1, response.Result.ProductionUnit.Industry.HistoricIndustries.Count());
        Assert.AreEqual(0, response.Result.ProductionUnit.Industry.SecondaryIndustries.Count());
        Assert.AreEqual(0, response.Result.ProductionUnit.Industry.HistoricSecondaryIndustries.Count());
    }

    [TestMethod]
    public async Task GetProductionUnitWhenStatusesTest()
    {
        const string PRODUCTION_UNIT_ID = "1002927924";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.IsNotNull(response.Result.ProductionUnit.Status);
        Assert.AreEqual("Normal", response.Result.ProductionUnit.Status.Current.Value);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenStatusesAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1007636896";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual("Ophørt", response.Result.ProductionUnit.Status.Current.Value);
        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenEmployeesTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsTrue(response.Result.ProductionUnit.Employees.Current.NumberOfEmployees > 400);
        Assert.IsTrue(response.Result.ProductionUnit.Employees.Current.NumberOfEmployees < 600);
        Assert.IsTrue(response.Result.ProductionUnit.Employees.Current.NumberOfFulltimePositions > 400);
        Assert.IsTrue(response.Result.ProductionUnit.Employees.Current.NumberOfFulltimePositions < 600);
        Assert.IsNotNull(response.Result.ProductionUnit.Employees.Current.UpdatedAt);
        Assert.IsTrue(response.Result.ProductionUnit.Employees.HistoricEmployees.Count() > 90);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenEmployeesAndInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1022444316";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsFalse(response.Result.ProductionUnit.Status.IsActive);
        Assert.IsNull(response.Result.ProductionUnit.Employees);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenCompaniesTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.AreEqual(1, response.Result.ProductionUnit.Companies.Current.Count());

        var company = response.Result.ProductionUnit.Companies.Current.FirstOrDefault();
        Assert.IsNotNull(company);
        Assert.AreEqual("13612870", company.RegistrationNumber);
        Assert.IsNotNull(company.Period.From);
        Assert.IsNull(company.Period.To);
        Assert.IsNotNull(company.UpdatedAt);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenCompaniesWhenInactiveTest()
    {
        const string PRODUCTION_UNIT_ID = "1022444316";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.AreEqual(0, response.Result.ProductionUnit.Companies.Current.Count());
        Assert.AreEqual(1, response.Result.ProductionUnit.Companies.HistoricCompanies.Count());

        var company = response.Result.ProductionUnit.Companies.HistoricCompanies.FirstOrDefault();
        Assert.IsNotNull(company);
        Assert.AreEqual("38641875", company.RegistrationNumber);
        Assert.IsNotNull(company.Period.From);
        Assert.IsNotNull(company.Period.To);
        Assert.IsNotNull(company.UpdatedAt);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenFoundedAtTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsNotNull(response.Result.ProductionUnit.FoundedAt);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenDissolvedAtTest()
    {
        const string PRODUCTION_UNIT_ID = "1022444316";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsNotNull(response.Result.ProductionUnit.DissolvedAt);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenIsHeaduartersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenIsSupportingUnitTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenIsTemporaryTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenHasConfidentialityTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenIsProtectedFromAdvertisementTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenEntityTypeTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.AreEqual(EntityType.ProductionUnit, response.Result.ProductionUnit.EntityType);
    }

    [TestMethod]
    public async Task GetProductionUnitWhenImportErrorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenRegistrationErrorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetProductionUnitWhenUpdatedAtTest()
    {
        const string PRODUCTION_UNIT_ID = "1000594476";

        var response = await this.DanishCvrService
            .GetProductionUnitAsync(PRODUCTION_UNIT_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        
        Assert.IsNotNull(response.Result.ProductionUnit.UpdatedAt);
    }
}