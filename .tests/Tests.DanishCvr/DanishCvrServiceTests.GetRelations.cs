using System;
using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Requests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task GetRelationsWhenPersonTest()
    {
        const string PERSON_EXTERNAL_ID = "4005942274";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_EXTERNAL_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(1, response.TotalResults);

        Assert.AreEqual(1, response.Result.Companies.Count());
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndHistoricTest()
    {
        const string PERSON_EXTERNAL_ID = "4005942274";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_EXTERNAL_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(5, response.TotalResults);

        Assert.AreEqual(5, response.Result.Companies.Count());
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndPagingTest()
    {
        const string PERSON_EXTERNAL_ID = "4005942274";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_EXTERNAL_ID, true, new Paging { Count = 2, Skip = 2 });

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(5, response.TotalResults);

        Assert.AreEqual(2, response.Result.Companies.Count());

        var company = response.Result.Companies.FirstOrDefault();
        Assert.IsNotNull(company);
        Assert.AreEqual("38696769", company.RegistrationNumber);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndOrderedCorrectlyTest()
    {
        const string PERSON_EXTERNAL_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_EXTERNAL_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var result = response.Result.Companies
            .Select(x => x.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) <= 0, $"The list is not in correct order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleFoundersTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var founders = response.Result.Companies
            .Select(x => x.Roles.Founder)
            .Where(x => x != null)
            .ToArray();

        Assert.AreEqual(2, founders.Length);

        var founder = founders.FirstOrDefault();
        Assert.IsNotNull(founder);
        Assert.AreEqual("36542993", founder.RegistrationNumber);
        Assert.IsNotNull(founder.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleLegalOwnersTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var legalOwners = response.Result.Companies
            .Select(x => x.Roles.LegalOwner)
            .Where(x => x != null)
            .ToArray();

        Assert.AreEqual(2, legalOwners.Length);

        var legalOwner = legalOwners.FirstOrDefault();
        Assert.IsNotNull(legalOwner);
        Assert.AreEqual("36542993", legalOwner.RegistrationNumber);
        Assert.IsNotNull(legalOwner.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleBeneficialOwnersTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var beneficialOwners = response.Result.Companies
            .Select(x => x.Roles.BeneficialOwner)
            .Where(x => x != null)
            .ToArray();

        Assert.AreEqual(4, beneficialOwners.Length);

        var beneficialOwner = beneficialOwners.FirstOrDefault();
        Assert.IsNotNull(beneficialOwner);
        Assert.IsNotNull(beneficialOwner.Equity);
        Assert.IsNotNull(beneficialOwner.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleLiquidatorsTest()
    {
        const string PERSON_ID = "4004056572";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var liquidators = response.Result.Companies
            .Select(x => x.Roles.Liquidator)
            .Where(y => y != null)
            .ToArray();

        Assert.AreEqual(6, liquidators.Length);

        var liquidator = liquidators.FirstOrDefault();
        Assert.IsNotNull(liquidator);
        Assert.AreEqual("Likvidator", liquidator.Title);
        Assert.AreEqual("Skifteretten", liquidator.AppointedBy);
        Assert.IsNotNull(liquidator.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleAuditorsExecutivesTest()
    {
        const string PERSON_ID = "4000509800";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var executives = response.Result.Companies
            .Where(x => x.Roles.Executives != null)
            .SelectMany(x => x.Roles.Executives.HistoricExecutives
                .Concat([x.Roles.Executives.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(3, executives.Length);

        var executive = executives.FirstOrDefault();
        Assert.IsNotNull(executive);
        Assert.AreEqual("Direktør", executive.Title);
        Assert.IsNotNull(executive.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleBoardMembersTest()
    {
        const string PERSON_ID = "4003990349";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var boardMembers = response.Result.Companies
            .Where(x => x.Roles.BoardMembers != null)
            .SelectMany(x => x.Roles.BoardMembers.HistoricBoardMembers
                .Concat([x.Roles.BoardMembers.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(12, boardMembers.Length);

        var boardMember = boardMembers.FirstOrDefault();
        Assert.IsNotNull(boardMember);
        Assert.AreEqual("Formand", boardMember.Title);
        Assert.IsNull(boardMember.ElectionMethod);
        Assert.IsNull(boardMember.AlternateFor);
        Assert.IsFalse(boardMember.IsDirective8Approved);
        Assert.IsNotNull(boardMember.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleManagersTest()
    {
        const string PERSON_ID = "4004026607";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var managers = response.Result.Companies
            .Where(x => x.Roles.Managers != null)
            .SelectMany(x => x.Roles.Managers.HistoricManagers
                .Concat([x.Roles.Managers.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(1, managers.Length);

        var manager = managers.FirstOrDefault();
        Assert.IsNotNull(manager);
        Assert.AreEqual("Daglig ledelse", manager.Title);
        Assert.IsNull(manager.ElectionMethod);
        Assert.IsNotNull(manager.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleAntiMoneyLaunderingAppointeesTest()
    {
        const string PERSON_ID = "4000181695";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var moneyLaunderingAppointees = response.Result.Companies
            .Where(x => x.Roles.AntiMoneyLaunderingAppointees != null)
            .SelectMany(x => x.Roles.AntiMoneyLaunderingAppointees.HistoricAntiMoneyLaunderingAppointees
                .Concat([x.Roles.AntiMoneyLaunderingAppointees.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(1, moneyLaunderingAppointees.Length);

        var moneyLaunderingAppointee = moneyLaunderingAppointees.FirstOrDefault();
        Assert.IsNotNull(moneyLaunderingAppointee);
        Assert.AreEqual("Leder, Reel ejer", moneyLaunderingAppointee.Title);
        Assert.IsNotNull(moneyLaunderingAppointee.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleAuthorizedSignatoriesTest()
    {
        const string PERSON_ID = "4000440948";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var authorizedSignatories = response.Result.Companies
            .SelectMany(x => x.Roles.AuthorizedSignatories.HistoricAuthorizedSignatories
                .Concat([x.Roles.AuthorizedSignatories.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(1, authorizedSignatories.Length);

        var authorizedSignatory = authorizedSignatories.FirstOrDefault();
        Assert.IsNotNull(authorizedSignatory);
        Assert.IsNotNull(authorizedSignatory.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleSpecialFinancialParticipantsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive("Persons can't be Special Financial Participants");
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleLiableParticipantsTest()
    {
        const string PERSON_ID = "4000148864";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var liableParticipants = response.Result.Companies
            .Where(x => x.Roles.LiableParticipants != null)
            .SelectMany(x => x.Roles.LiableParticipants.HistoricLiableParticipants
                .Concat([x.Roles.LiableParticipants.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(1, liableParticipants.Length);

        var liableParticipant = liableParticipants.FirstOrDefault();
        Assert.IsNotNull(liableParticipant);
        Assert.AreEqual("Interessenter", liableParticipant.Role);
        Assert.IsNull(liableParticipant.ElectionMethod);
        Assert.IsNull(liableParticipant.RegisteredCapital);
        Assert.IsNotNull(liableParticipant.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleAssociationRepresentativesTest()
    {
        const string PERSON_ID = "4009632928";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var associationRepresentatives = response.Result.Companies
            .SelectMany(x => x.Roles.AssociationRepresentatives.HistoricAssociationRepresentatives
                .Concat([x.Roles.AssociationRepresentatives.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(25, associationRepresentatives.Length);

        var associationRepresentative = associationRepresentatives.FirstOrDefault();
        Assert.IsNotNull(associationRepresentative);
        Assert.AreEqual("Foreningsrepræsentant", associationRepresentative.Title);
        Assert.IsNotNull(associationRepresentative.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenPersonAndCompanyRoleCertifiedAuditorsTest()
    {
        const string PERSON_ID = "4000014983";

        var response = await this.DanishCvrService
            .GetRelationsAsync(PERSON_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var certifiedAuditors = response.Result.Companies
            .Where(x => x.Roles.CertifiedAuditors != null)
            .SelectMany(x => x.Roles.CertifiedAuditors.HistoricCertifiedAuditors
                .Concat([x.Roles.CertifiedAuditors.Current])
                .Where(y => y != null))
            .ToArray();

        Assert.AreEqual(2, certifiedAuditors.Length);

        var certifiedAuditor = certifiedAuditors.FirstOrDefault();
        Assert.IsNotNull(certifiedAuditor);
        Assert.AreEqual("Revisionsvirksomhed Stemmeberettiget", certifiedAuditor.Role);
        Assert.AreEqual("Weidekampsgade 6, 2300 københavn s", certifiedAuditor.BusinessAddress);
        Assert.IsNotNull(certifiedAuditor.VotingRights);
        Assert.IsNotNull(certifiedAuditor.UpdatedAt);
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyTest()
    {
        const string COMPANY_EXTERNAL_ID = "4005901124";

        var response = await this.DanishCvrService
            .GetRelationsAsync(COMPANY_EXTERNAL_ID);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(3, response.TotalResults);

        Assert.AreEqual(3, response.Result.Companies.Count());
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndHistoricTest()
    {
        const string COMPANY_EXTERNAL_ID = "4005901124";

        var response = await this.DanishCvrService
            .GetRelationsAsync(COMPANY_EXTERNAL_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(11, response.TotalResults);

        Assert.AreEqual(11, response.Result.Companies.Count());
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndPagingTest()
    {
        const string COMPANY_EXTERNAL_ID = "4005901124";

        var response = await this.DanishCvrService
            .GetRelationsAsync(COMPANY_EXTERNAL_ID, false, new Paging { Count = 2, Skip = 2 });

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        Assert.AreEqual(1, response.Result.Companies.Count());

        var company = response.Result.Companies.FirstOrDefault();
        Assert.IsNotNull(company);
        Assert.AreEqual("43804146", company.RegistrationNumber);
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndOrderedCorrectlyTest()
    {
        const string COMPANY_EXTERNAL_ID = "4005901124";

        var response = await this.DanishCvrService
            .GetRelationsAsync(COMPANY_EXTERNAL_ID, true);

        await this.SaveJsonOrDefault(response.Result);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Result);

        var result = response.Result.Companies
            .Select(x => x.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) <= 0, $"The list is not in correct order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleFoundersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleLegalOwnersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleBeneficialOwnersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleLiquidatorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleAuditorsExecutivesTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleBoardMembersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleManagersTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleAntiMoneyLaunderingAppointeesTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleAuthorizedSignatoriesTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleSpecialFinancialParticipantsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleLiableParticipantsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleAssociationRepresentativesTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }

    [TestMethod]
    public async Task GetRelationsWhenCompanyAndCompanyRoleCertifiedAuditorsTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();
    }
}