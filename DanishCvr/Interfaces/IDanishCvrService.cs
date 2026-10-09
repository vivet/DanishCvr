using DanishCvr.Requests;
using DanishCvr.Requests.Models;
using DanishCvr.Responses;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DanishCvr.Interfaces;

/// <summary>
/// Danish Cvr Service Interface.
/// https://assets.eu.ctfassets.net/i6cea2ilpb83/5StPtRVFA1oMM2XkrpVx1G/4235f52ed303a4663b699cb2bb197347/soegeeksempler_permanent_v.6.x.pdf
/// https://assets.ctfassets.net/xooti71hnsox/3AmPSE0KQtb5UDFWRnxgx3/45bb296d572e4e6e00f268e49e4c7f92/S__geeksempler_i_Registreringstekster.pdf
/// </summary>
public interface IDanishCvrService
{
    /// <summary>
    /// Get Company Business Types Async.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="BusinessTypesResponse"/>.</returns>
    Task<BusinessTypesResponse> GetCompanyBusinessTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Company Statuses Async.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="StatusesResponse"/>.</returns>
    Task<StatusesResponse> GetCompanyStatusesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Company Industries Async.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IndustriesResponse"/>.</returns>
    Task<IndustriesResponse> GetCompanyIndustriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Company Async.
    /// </summary>
    /// <param name="registrationNumber">The registration number.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="CompanyResponse"/>.</returns>
    Task<CompanyResponse> GetCompanyAsync(string registrationNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Company By External Id Async.
    /// </summary>
    /// <param name="externalId">The external id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="CompanyResponse"/>.</returns>
    Task<CompanyResponse> GetCompanyByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Companies Async.
    /// </summary>
    /// <param name="registrationNumbers">The registration numbers.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="CompaniesSearchResponse"/>.</returns>
    Task<CompaniesSearchResponse> GetCompaniesAsync(IEnumerable<string> registrationNumbers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Companies Async.
    /// </summary>
    /// <param name="registrationNumberPrefix">The registration number prefix. Wilcard (*) will be appended.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="CompaniesResponse"/>.</returns>
    Task<CompaniesResponse> GetCompaniesAsync(string registrationNumberPrefix, Paging paging = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto Complete Companies Async.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="CompaniesAutoCompleteResponse"/>.</returns>
    Task<CompaniesAutoCompleteResponse> AutoCompleteCompaniesAsync(AutoCompleteCompaniesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search Company Async.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="CompaniesSearchResponse"/>.</returns>
    Task<CompaniesSearchResponse> SearchCompaniesAsync(SearchCompaniesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Company Registration Texts Async.
    /// </summary>
    /// <param name="registrationNumber">The registration number.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="RegistrationTextsResponse"/>.</returns>
    Task<RegistrationTextsResponse> GetCompanyRegistrationTextsAsync(string registrationNumber, Paging paging = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Production Unit Async.
    /// </summary>
    /// <param name="productionUnitId">The production unit id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="ProductionUnitResponse"/>.</returns>
    Task<ProductionUnitResponse> GetProductionUnitAsync(string productionUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Production Units Async.
    /// </summary>
    /// <param name="registrationNumber">The registration number.</param>
    /// <param name="includeHistoric">Whether to include historic inactive production units.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The collection of <see cref="ProductionUnitResponse"/>.</returns>
    Task<ProductionUnitsResponse> GetProductionUnitsAsync(string registrationNumber, bool includeHistoric = false, Paging paging = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search Production Units Async.
    /// </summary>
    /// <param name="request">The <see cref="SearchProductionUnitsRequest"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="ProductionUnitsSearchResponse"/>.</returns>
    Task<ProductionUnitsSearchResponse> SearchProductionUnitsAsync(SearchProductionUnitsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Person Async.
    /// </summary>
    /// <param name="externalId">The external id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="PersonResponse"/>.</returns>
    Task<PersonResponse> GetPersonAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto Complete Persons Async.
    /// </summary>
    /// <param name="request">The <see cref="AutoCompletePersonsRequest"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="PersonsAutoCompleteResponse"/>.</returns>
    Task<PersonsAutoCompleteResponse> AutoCompletePersonsAsync(AutoCompletePersonsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search Persons Async.
    /// </summary>
    /// <param name="request">The <see cref="SearchPersonsRequest"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="PersonsSearchResponse"/>.</returns>
    Task<PersonsSearchResponse> SearchPersonsAsync(SearchPersonsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Relations Async.
    /// </summary>
    /// <param name="externalId">The external id.</param>
    /// <param name="includeHistoric">Whether to include historic inactive relations.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="RelationsResponse"/>.</returns>
    Task<RelationsResponse> GetRelationsAsync(string externalId, bool includeHistoric = false, Paging paging = null, CancellationToken cancellationToken = default);
}