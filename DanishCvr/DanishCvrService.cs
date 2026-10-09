using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DanishCvr.Consts;
using DanishCvr.Extensions;
using DanishCvr.Interfaces;
using DanishCvr.Responses;
using DanishCvr.Helpers;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using ExistsQuery = Nest.ExistsQuery;
using Industries = DanishCvr.Responses.Models.Industries;
using MergersAndSplits = DanishCvr.Responses.Models.MergersAndSplits;
using Name = DanishCvr.Responses.Models.Name;
using Names = DanishCvr.Responses.Models.Names;
using Period = DanishCvr.Types.Period;
using SearchPersonsRequest = DanishCvr.Requests.SearchPersonsRequest;
using Status = DanishCvr.Responses.Models.Status;
using DanishCvr.Models;
using DanishCvr.Requests;
using DanishCvr.Responses.Models;
using DanishCvr.Responses.Models.Enums;
using DanishCvr.Requests.Models;
using DanishCvr.Requests.Models.Enums;

namespace DanishCvr;

/// <inheritdoc />
public class DanishCvrService : IDanishCvrService
{
    private const string INDEX_CVR_PERMANENT = "cvr-permanent";
    private const string INDEX_REGISTRERINGS_TEKSTER = "registreringstekster";

    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Converters =
        {
            new StringEnumConverter()
        }
    };

    private static readonly IEnumerable<IndustryResult> industryResults;
    private static readonly IEnumerable<string> statusResults = new List<string>
    {
        "Fremtid",
        "Normal",
        "Ophørt",
        "Opløst",
        "Opløst efter erklæring",
        "Opløst efter frivillig likvidation",
        "Opløst efter fusion",
        "Opløst efter grænseoverskridende fusion",
        "Opløst efter grænseoverskridende hjemstedsflytning",
        "Opløst efter konkurs",
        "Opløst efter spaltning",
        "Slettet",
        "Tvangsopløst",
        "Under frivillig likvidation",
        "Under konkurs",
        "Under reassumering",
        "Under rekonstruktion",
        "Under tvangsopløsning"
    };

    private static readonly NumberFormatInfo numberFormatInfo = new()
    {
        CurrencyDecimalSeparator = "."
    };

    private readonly ILogger logger;
    private readonly ElasticClient elasticClient;
    private readonly DanishCvrOptions options;

    static DanishCvrService()
    {
        DanishCvrService.industryResults = typeof(DanishCvrService).Assembly.GetJsonResource<IEnumerable<IndustryResult>>("DanishCvr.Resources.branchekoder.json");
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="elasticClient">The <see cref="ElasticClient"/>.</param>
    /// <param name="options">The <see cref="DanishCvrOptions"/>.</param>
    public DanishCvrService(ILogger logger, ElasticClient elasticClient, DanishCvrOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    /// <inheritdoc />
    public virtual async Task<StatusesResponse> GetCompanyStatusesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return new StatusesResponse
        {
            Results = DanishCvrService.statusResults
                .Select(x => new StatusResult
                {
                    Text = x.ToStringPretty()
                })
                .OrderBy(x => x.Text),
            TotalResults = DanishCvrService.statusResults.Count()
        };
    }

    /// <inheritdoc />
    public virtual async Task<BusinessTypesResponse> GetCompanyBusinessTypesAsync(CancellationToken cancellationToken = default)
    {
        const string AGGREGATE_NAME = "distinct_field";
        const string AGGREGATE_INCLUDED_NAME = "included_fields";

        var includeFields = new List<Field>
        {
            new("Vrvirksomhed.virksomhedsform.virksomhedsformkode"),
            new("Vrvirksomhed.virksomhedsform.kortBeskrivelse"),
            new("Vrvirksomhed.virksomhedsform.langBeskrivelse")
        };

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(0)
                    .Aggregations(y => y
                        .Terms(AGGREGATE_NAME, z => z
                            .Field("Vrvirksomhed.virksomhedsform.virksomhedsformkode")
                            .Size(2500)
                            .Aggregations(a => a
                                .TopHits(AGGREGATE_INCLUDED_NAME, b => b
                                    .Size(1)
                                    .Source(c => c
                                        .Includes(d => d
                                            .Fields(includeFields)))))))
                    .Sort(y => y
                        .Ascending("Vrvirksomhed.virksomhedsform.virksomhedsformkode"))
                ,
                cancellationToken);

        var termsAggregation = response.Aggregations
            .Terms(AGGREGATE_NAME);

        var results = termsAggregation?.Buckets
            .Select(x =>
            {
                var code = x.Key.ToString();

                var source = x
                    .TopHits(AGGREGATE_INCLUDED_NAME)
                    .Hits<JToken>()
                    .Select(y => y.Source)
                    .FirstOrDefault();

                if (source?["Vrvirksomhed"]?["virksomhedsform"] is JArray jArray && jArray.Any())
                {
                    var matchingForm = jArray
                        .FirstOrDefault(y => y?["virksomhedsformkode"]?.ToString() == code);

                    var abbreviation = matchingForm?["kortBeskrivelse"]?.ToString();
                    var description = matchingForm?["langBeskrivelse"]?.ToString();

                    return new BusinessTypeResult
                    {
                        Code = code,
                        Abbreviation = abbreviation,
                        Description = description
                    };
                }

                return null;
            })
            .Where(x => x != null)
            .OrderBy(x => int.Parse(x.Code))
            .ToList();

        return new BusinessTypesResponse
        {
            Results = results ?? [],
            TotalResults = results?.Count ?? 0
        };
    }

    /// <inheritdoc />
    public virtual async Task<IndustriesResponse> GetCompanyIndustriesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return new IndustriesResponse
        {
            Results = DanishCvrService.industryResults
                .OrderBy(x => int.Parse(x.Code)),
            TotalResults = DanishCvrService.industryResults.Count()
        };
    }

    /// <inheritdoc />
    public virtual async Task<CompanyResponse> GetCompanyAsync(string registrationNumber, CancellationToken cancellationToken = default)
    {
        if (registrationNumber == null)
            throw new ArgumentNullException(nameof(registrationNumber));

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(1)
                    .Query(y => y
                        .Bool(z => z
                            .Must(a => a
                                .Term(b => b
                                    .Field("Vrvirksomhed.cvrNummer")
                                    .Value(registrationNumber))))),
                cancellationToken);

        var result = response.Hits
            .Select(x => this.MapCompanyResult(x.Source.VrVirksomhed))
            .FirstOrDefault();

        return new CompanyResponse
        {
            Result = result
        };
    }

    /// <inheritdoc />
    public virtual async Task<CompanyResponse> GetCompanyByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        if (externalId == null)
            throw new ArgumentNullException(nameof(externalId));

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(1)
                    .Query(y => y
                        .Bool(z => z
                            .Must(a => a
                                .Term(b => b
                                    .Field("Vrvirksomhed.enhedsNummer")
                                    .Value(externalId))))),
                cancellationToken);

        var result = response.Hits
            .Select(x => this.MapCompanyResult(x.Source.VrVirksomhed))
            .FirstOrDefault();

        return new CompanyResponse
        {
            Result = result
        };
    }

    /// <inheritdoc />
    public virtual async Task<CompaniesSearchResponse> GetCompaniesAsync(IEnumerable<string> registrationNumbers, CancellationToken cancellationToken = default)
    {
        if (registrationNumbers == null) 
            throw new ArgumentNullException(nameof(registrationNumbers));

        var terms = registrationNumbers
            .ToArray();

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(terms.Length)
                    .Query(y => y
                        .Terms(z => z
                            .Field("Vrvirksomhed.cvrNummer")
                            .Terms(terms))),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapCompanySearchResult(x.Source.VrVirksomhed));

        return new CompaniesSearchResponse
        {
            Results = results
        };
    }

    /// <inheritdoc />
    public virtual async Task<CompaniesResponse> GetCompaniesAsync(string registrationNumberPrefix, Paging paging = null, CancellationToken cancellationToken = default)
    {
        if (registrationNumberPrefix == null)
            throw new ArgumentNullException(nameof(registrationNumberPrefix));

        paging ??= new Paging();

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(paging.Count)
                    .Skip(paging.Skip)
                    .Query(y => y
                        .Bool(z => z.Must(a => a
                            .Wildcard(b => b
                                .Field("Vrvirksomhed.cvrNummer")
                                .Value($"{registrationNumberPrefix}*")))))
                    .Sort(y => y
                        .Ascending("Vrvirksomhed.cvrNummer")),
                cancellationToken);

        var results = response.Hits
            .Select(x =>
            {
                try
                {
                    return this.MapCompanyResult(x.Source.VrVirksomhed);
                }
                catch (Exception ex)
                {
                    this.logger
                        .LogWarning(ex, $"Registration Number: {x.Source.VrVirksomhed.CvrNummer}: {ex.Message}");

                    return null;
                }
            })
            .Where(x => x != null)
            .ToArray();

        return new CompaniesResponse
        {
            Results = results,
            TotalResults = response.Total
        };
    }

    /// <inheritdoc />
    public virtual async Task<CompaniesAutoCompleteResponse> AutoCompleteCompaniesAsync(AutoCompleteCompaniesRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var musts = new[]
            {
                this.GetCompaniesAutoCompleteQueryName(request),
                this.GetCompaniesAutoCompleteIsActive(request)
            }
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Source(y => y
                        .Includes(z => z
                            .Field("Vrvirksomhed.cvrNummer")
                            .Field("Vrvirksomhed.navne")
                            .Field("Vrvirksomhed.virksomhedsform")
                            .Field("Vrvirksomhed.beliggenhedsadresse")
                            .Field("Vrvirksomhed.livsforloeb")
                            .Field("Vrvirksomhed.virksomhedMetadata.nyesteNavn.navn")
                            .Field("Vrvirksomhed.virksomhedMetadata.nyesteBeliggenhedsadresse")
                            .Field("Vrvirksomhed.virksomhedMetadata.nyesteVirksomhedsform")))
                    .Size(10)
                    .Query(y => y
                        .Bool(z => z
                            .Must(musts)))
                    .Sort(y => y
                        .Ascending(new Field("Vrvirksomhed.virksomhedMetadata.nyesteNavn.navn_sort"))),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapCompanyAutoCompleteResult(x.Source.VrVirksomhed))
            .ToArray();

        return new CompaniesAutoCompleteResponse
        {
            Results = results
        };
    }

    /// <inheritdoc />
    public virtual async Task<CompaniesSearchResponse> SearchCompaniesAsync(SearchCompaniesRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var musts = new[]
            {
                this.GetCompaniesSearchQueryName(request),
                this.GetCompaniesSearchQueryBusinessCodes(request),
                this.GetCompaniesSearchQueryZipCodes(request),
                this.GetCompaniesSearchQueryIndustryCodes(request),
                this.GetCompaniesSearchQueryStatus(request),
                this.GetCompaniesSearchQueryEmployeesFrom(request),
                this.GetCompaniesSearchQueryEmployeesTo(request),
                this.GetCompaniesSearchQueryFoundedAtFrom(request),
                this.GetCompaniesSearchQueryFoundedAtTo(request),
                this.GetCompaniesSearchQueryDissolvedAtFrom(request),
                this.GetCompaniesSearchQueryDissolvedAtTo(request),
                this.GetCompaniesSearchQueryIsActive(request),
                this.GetCompaniesSearchQueryIsProtectedFromAdvertisment(request)
            }
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();

        var response = await this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(request.Paging.Count)
                    .Skip(request.Paging.Skip)
                    .Query(y => y
                        .Bool(z => z
                            .Must(musts)))
                    .Sort(y =>
                    {
                        var field = request.Sorting.By switch
                        {
                            CompanySortBy.Relevance => null,
                            CompanySortBy.Name => new Field("Vrvirksomhed.virksomhedMetadata.nyesteNavn.navn_sort"),
                            CompanySortBy.NumberOfEmployees => new Field("Vrvirksomhed.virksomhedMetadata.nyesteErstMaanedsbeskaeftigelse.antalAnsatte"),
                            CompanySortBy.FoundedAt => new Field("Vrvirksomhed.virksomhedMetadata.stiftelsesDato"),
                            CompanySortBy.DissolvedAt => new Field("Vrvirksomhed.livsforloeb.periode.gyldigTil"),
                            _ => null
                        };

                        if (field == null)
                        {
                            return null;
                        }

                        return request.Sorting.Direction == SortDirection.Ascending
                            ? y.Ascending(field)
                            : y.Descending(field);
                    }),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapCompanySearchResult(x.Source.VrVirksomhed))
            .ToArray();

        return new CompaniesSearchResponse
        {
            Results = results,
            TotalResults = response.Total
        };
    }

    /// <inheritdoc />
    public virtual async Task<RegistrationTextsResponse> GetCompanyRegistrationTextsAsync(string registrationNumber, Paging paging = null, CancellationToken cancellationToken = default)
    {
        if (registrationNumber == null)
            throw new ArgumentNullException(nameof(registrationNumber));

        paging ??= new Paging();

        var response = await this.elasticClient
            .SearchAsync<RegistreringsTekst>(x => x
                    .Index(INDEX_REGISTRERINGS_TEKSTER)
                    .Size(paging.Count)
                    .Skip(paging.Skip)
                    .Query(y => y
                        .Bool(z => z
                            .Must(a => a
                                .Term(b => b
                                    .Field("cvrNummer")
                                    .Value(registrationNumber)))))
                    .Sort(y => y
                        .Descending("registreringTidsstempel")),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapRegistrationTextResult(x.Source));

        return new RegistrationTextsResponse
        {
            Results = results,
            TotalResults = response.Total
        };
    }

    /// <inheritdoc />
    public virtual async Task<ProductionUnitResponse> GetProductionUnitAsync(string productionUnitid, CancellationToken cancellationToken = default)
    {
        if (productionUnitid == null)
            throw new ArgumentNullException(nameof(productionUnitid));

        var response = await this.elasticClient
            .SearchAsync<ProduktionsEnhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(1)
                    .Query(y => y
                        .Bool(z => z
                            .Must(a => a
                                .Term(b => b
                                    .Field("VrproduktionsEnhed.pNummer")
                                    .Value(productionUnitid))))),
                cancellationToken);

        var result = response.Hits
            .Select(x => this.MapProductionUnitResult(x.Source.VrProduktionsEnhed))
            .FirstOrDefault();

        return new ProductionUnitResponse
        {
            Result = result
        };
    }

    /// <inheritdoc />
    public virtual async Task<ProductionUnitsResponse> GetProductionUnitsAsync(string registrationNumber, bool includeHistoric = false, Paging paging = null, CancellationToken cancellationToken = default)
    {
        if (registrationNumber == null)
            throw new ArgumentNullException(nameof(registrationNumber));

        paging ??= new Paging();

        var response = await this.elasticClient
            .SearchAsync<ProduktionsEnhed>(x => x   
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(paging.Count)
                    .Skip(paging.Skip)
                    .Query(y => y
                        .Term(z => z
                            .Field("VrproduktionsEnhed.produktionsEnhedMetadata.nyesteCvrNummerRelation")
                            .Value(registrationNumber)))
                    .Sort(y => y
                        .Ascending("VrproduktionsEnhed.produktionsEnhedMetadata.nyesteNavn.navn_sort")),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapProductionUnitResult(x.Source.VrProduktionsEnhed));

        return new ProductionUnitsResponse
        {
            Results = results,
            TotalResults = response.Total
        };
    }

    /// <inheritdoc />
    public virtual async Task<ProductionUnitsSearchResponse> SearchProductionUnitsAsync(SearchProductionUnitsRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var musts = new[]
            {
                this.GetProductionUnitsSearchQueryName(request),
                this.GetProductionUnitsSearchQueryZipCodes(request),
                this.GetProductionUnitsSearchQueryIndustryCodes(request),
                this.GetProductionUnitsSearchQueryStatus(request),
                this.GetProductionUnitsSearchQueryEmployeesFrom(request),
                this.GetProductionUnitsSearchQueryEmployeesTo(request),
                this.GetProductionUnitsSearchQueryIsActive(request)
            }
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();

        var response = await this.elasticClient
            .SearchAsync<ProduktionsEnhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(request.Paging.Count)
                    .Skip(request.Paging.Skip)
                    .Query(y => y
                        .Bool(z => z
                            .Must(musts)))
                    .Sort(y =>
                    {
                        var field = request.Sorting.By switch
                        {
                            ProductionUnitSortBy.Relevance => null,
                            ProductionUnitSortBy.Name => new Field("VrproduktionsEnhed.produktionsEnhedMetadata.nyesteNavn.navn_sort"),
                            ProductionUnitSortBy.NumberOfEmployees => new Field("VrproduktionsEnhed.produktionsEnhedMetadata.nyesteErstMaanedsbeskaeftigelse.antalAnsatte"),
                            _ => null
                        };

                        if (field == null)
                        {
                            return null;
                        }

                        return request.Sorting.Direction == SortDirection.Ascending
                            ? y.Ascending(field)
                            : y.Descending(field);
                    }),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapProductionUnitSearchResult(x.Source.VrProduktionsEnhed));

        return new ProductionUnitsSearchResponse
        {
            Results = results,
            TotalResults = response.Total
        };
    }

    /// <inheritdoc />
    public virtual async Task<PersonResponse> GetPersonAsync(string externalId, CancellationToken cancellationToken = default)
    {
        if (externalId == null)
            throw new ArgumentNullException(nameof(externalId));

        var responsePerson = await this.elasticClient
            .SearchAsync<Deltager>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(1)
                    .Query(y => y
                        .Bool(z => z
                            .Must(a => a
                                .Term(b => b
                                    .Field("Vrdeltagerperson.enhedsNummer")
                                    .Value(externalId))))),
                cancellationToken);

        var resultPerson = responsePerson.Hits
            .Select(x => this.MapPersonResult(x.Source.VrDeltagerPerson))
            .FirstOrDefault();

        return new PersonResponse
        {
            Result = resultPerson
        };
    }

    /// <inheritdoc />
    public virtual async Task<PersonsAutoCompleteResponse> AutoCompletePersonsAsync(AutoCompletePersonsRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Name == null)
        {
            return null;
        }

        var nameQuery = this.GetPersonsAutoCompleteQueryName(request);

        if (nameQuery == null)
        {
            return null;
        }

        var response = await this.elasticClient
            .SearchAsync<Deltager>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Source(y => y
                        .Includes(z => z
                            .Field("Vrdeltagerperson.enhedsNummer")
                            .Field("Vrdeltagerperson.navne.navn")
                            .Field("Vrdeltagerperson.stilling")
                            .Field("Vrdeltagerperson.beliggenhedsadresse.postnummer")
                            .Field("Vrdeltagerperson.beliggenhedsadresse.postdistrikt")))
                    .Size(25)
                    .Query(y => y
                        .Bool(z => z
                            .Must(nameQuery)))
                    .Sort(y => y
                        .Ascending(new Field("Vrdeltagerperson.navne.navn_sort"))),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapPersonAutoCompleteResult(x.Source.VrDeltagerPerson));

        return new PersonsAutoCompleteResponse
        {
            Results = results
        };
    }

    /// <inheritdoc />
    public virtual async Task<PersonsSearchResponse> SearchPersonsAsync(SearchPersonsRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) 
            throw new ArgumentNullException(nameof(request));

        var musts = new[]
            {
                this.GetPersonsSearchQueryName(request),
                this.GetPersonsSearchQueryZipCodes(request)
            }
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();

        var response = await this.elasticClient
            .SearchAsync<Deltager>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Source(y => y
                        .Includes(z => z
                            .Field("Vrdeltagerperson.enhedsNummer")
                            .Field("Vrdeltagerperson.navne.navn")
                            .Field("Vrdeltagerperson.beliggenhedsadresse")
                            .Field("Vrdeltagerperson.telefonNummer")
                            .Field("Vrdeltagerperson.telefaxNummer")
                            .Field("Vrdeltagerperson.elektroniskPost")
                            .Field("Vrdeltagerperson.stilling")
                            .Field("Vrdeltagerperson.enhedstype")
                            .Field("Vrdeltagerperson.sidstOpdateret")))
                    .Size(request.Paging.Count)
                    .Skip(request.Paging.Skip)
                    .Query(y => y
                        .Bool(z => z
                            .Must(musts)))
                    .Sort(y =>
                    {
                        var field = request.Sorting.By switch
                        {
                            PersonSortBy.Name => new Field("Vrdeltagerperson.navne.navn_sort"),
                            _ => null
                        };

                        if (field == null)
                        {
                            return null;
                        }

                        return request.Sorting.Direction == SortDirection.Ascending
                            ? y.Ascending(field)
                            : y.Descending(field);
                    }),
                cancellationToken);

        var results = response.Hits
            .Select(x => this.MapPersonSearchResult(x.Source.VrDeltagerPerson));

        return new PersonsSearchResponse
        {
            Results = results,
            TotalResults = response.Total
        };
    }

    /// <inheritdoc />
    public virtual async Task<RelationsResponse> GetRelationsAsync(string externalId, bool includeHistoric = false, Paging paging = null, CancellationToken cancellationToken = default)
    {
        if (externalId == null)
            throw new ArgumentNullException(nameof(externalId));

        paging ??= new Paging();

        var responsePersonTask = this.elasticClient
            .SearchAsync<Deltager>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Source(y => y
                        .Includes(z => z
                            .Field("Vrdeltagerperson.virksomhedSummariskRelation")))
                    .Size(1)
                    .Query(y => y
                        .Bool(z => z
                            .Must(a => a
                                .Term(b => b
                                    .Field("Vrdeltagerperson.enhedsNummer")
                                    .Value(externalId))))),
                cancellationToken);

        var musts = new List<QueryContainer>
        {
            new TermQuery
            {
                Field = "Vrvirksomhed.deltagerRelation.deltager.enhedsNummer",
                Value = externalId
            }
        };

        if (!includeHistoric)
        {
            musts
                .Add(new BoolQuery
                {
                    MustNot =
                    [
                        new ExistsQuery
                        {
                            Field = "Vrvirksomhed.livsforloeb.periode.gyldigTil"
                        }
                    ]
                });

            musts
                .Add(new NestedQuery
                {
                    Path = "Vrvirksomhed.deltagerRelation",
                    Query = new BoolQuery
                    {
                        Must =
                        [
                            new TermQuery
                            {
                                Field = "Vrvirksomhed.deltagerRelation.deltager.enhedsNummer",
                                Value = externalId
                            },
                            new NestedQuery
                            {
                                Path = "Vrvirksomhed.deltagerRelation.organisationer.medlemsData.attributter.vaerdier",
                                Query = new BoolQuery
                                {
                                    MustNot =
                                    [
                                        new ExistsQuery
                                        {
                                            Field = "Vrvirksomhed.deltagerRelation.organisationer.medlemsData.attributter.vaerdier.periode.gyldigTil"
                                        }
                                    ]
                                }
                            }
                        ]
                    }
                });
        }

        var responseCompaniesTask = this.elasticClient
            .SearchAsync<Virksomhed>(x => x
                    .Index(INDEX_CVR_PERMANENT)
                    .Size(paging.Count) 
                    .Skip(paging.Skip)
                    .Query(y => y
                        .Bool(z => z
                            .Must(musts.ToArray())))
                    .Sort(y => y
                        .Ascending("Vrvirksomhed.virksomhedMetadata.nyesteNavn.navn_sort")),
                cancellationToken);

        var responsePerson = await responsePersonTask;
        var responseCompanies = await responseCompaniesTask;

        if (responsePerson.Hits.Any())
        {
            var resultPerson = responsePerson.Hits
                .Select(x => this.MapRelationResult(x.Source.VrDeltagerPerson, includeHistoric))
                .FirstOrDefault();

            if (resultPerson != null)
            {
                var totalResults = resultPerson.Companies.Count();

                resultPerson.Companies = resultPerson.Companies
                    .OrderBy(x => x.Name.Value)
                    .Skip(paging.Skip)
                    .Take(paging.Count);

                return new RelationsResponse
                {
                    Result = resultPerson,
                    TotalResults = totalResults
                };
            }
        }

        var virksomheder = responseCompanies.Hits
            .Select(x => x.Source.VrVirksomhed)
            .ToArray();

        var resultCompany = this.MapRelationResult(virksomheder, externalId, includeHistoric);

        if (resultCompany != null)
        {
            return new RelationsResponse
            {
                Result = resultCompany,
                TotalResults = responseCompanies.Total
            };
        }

        return null;
    }

    private QueryContainer[] GetCompaniesAutoCompleteQueryName(AutoCompleteCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var queryName = this.GetCompaniesSearchQueryName(new SearchCompaniesRequest
        {
            Criteria =
            {
                Names = new NamesWithAlternative
                {
                    Name = request.Names.Name,
                    IncludeAlternativeNames = request.Names.IncludeAlternativeNames,
                    IncludeHistoricNames = request.Names.IncludeHistoricNames
                }
            }
        });

        return queryName;
    }
    private QueryContainer[] GetCompaniesAutoCompleteIsActive(AutoCompleteCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var queryIsActive = this.GetCompaniesSearchQueryIsActive(new SearchCompaniesRequest
        {
            Criteria =
            {
                IsActive = request.IsActive
            }
        });

        return queryIsActive;
    }
    private QueryContainer[] GetCompaniesSearchQueryName(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: The search is based on tokenized text, why searches will not always return results and why the search isn't working similar to an SQL like '{text}%'.

        if (request.Criteria.Names.Name == null)
        {
            return null;
        }

        var shouldContainers = new List<QueryContainer>();

        var nameField = request.Criteria.Names.IncludeHistoricNames
            ? "Vrvirksomhed.navne.navn"
            : "Vrvirksomhed.virksomhedMetadata.nyesteNavn.navn";

        var alternativeNameField = request.Criteria.Names.IncludeHistoricNames
            ? "Vrvirksomhed.binavne.navn"
            : "Vrvirksomhed.virksomhedMetadata.nyesteBinavne";

        request.Criteria.Names.Name = request.Criteria.Names.Name.Trim();

        var lastIndexOfSpace = request.Criteria.Names.Name
            .LastIndexOf(' ');

        if (lastIndexOfSpace > -1)
        {
            var partOne = request.Criteria.Names.Name[..lastIndexOfSpace];
            var partTwo = request.Criteria.Names.Name[lastIndexOfSpace..];

            shouldContainers
                .Add(new BoolQuery
                {
                    Must =
                    [
                        new MatchPhraseQuery
                        {
                            Field = nameField,
                            Query = partOne
                        }
                    ],
                    Should = new List<QueryContainer>
                    {
                        new MatchPhraseQuery
                        {
                            Field = nameField,
                            Query = partTwo,
                            Boost = 4.0
                        },
                        new MatchPhrasePrefixQuery
                        {
                            Field = nameField,
                            Query = partTwo,
                            Boost = 2.0
                        }
                    },
                    MinimumShouldMatch = 1
                });

            if (request.Criteria.Names.IncludeAlternativeNames)
            {
                shouldContainers
                    .Add(new BoolQuery
                    {
                        Must =
                        [
                            new MatchPhraseQuery
                            {
                                Field = alternativeNameField,
                                Query = partOne
                            }
                        ],
                        Should = new List<QueryContainer>
                        {
                            new MatchPhraseQuery
                            {
                                Field = alternativeNameField,
                                Query = partTwo,
                                Boost = 4.0
                            },
                            new MatchPhrasePrefixQuery
                            {
                                Field = alternativeNameField,
                                Query = partTwo,
                                Boost = 2.0
                            }
                        },
                        MinimumShouldMatch = 1
                    });
            }
        }
        else
        {
            shouldContainers
                .Add(new MatchPhrasePrefixQuery
                {
                    Field = nameField,
                    Query = request.Criteria.Names.Name,
                    Boost = 2.0
                });

            shouldContainers
                .Add(new MatchPhrasePrefixQuery
                {
                    Field = alternativeNameField,
                    Query = request.Criteria.Names.Name
                });
        }

        return
        [
            new BoolQuery
            {
                Should = shouldContainers,
                MinimumShouldMatch = 1
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryBusinessCodes(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Criteria.BusinessTypeCodes.Any())
        {
            return null;
        }

        return
        [
            new TermsQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.nyesteVirksomhedsform.virksomhedsformkode",
                Terms = request.Criteria.BusinessTypeCodes
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryZipCodes(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.Within?.Location == null)
        {
            return null;
        }

        var postnumre = PostalCodes.GetPostalCodesWithin(request.Criteria.Within);

        return
        [
            new TermsQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.nyesteBeliggenhedsadresse.postnummer",
                Terms = postnumre
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryIndustryCodes(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Criteria.IndustryCodes.Any())
        {
            return null;
        }

        return
        [
            new BoolQuery
            {
                Should = new List<QueryContainer>
                {
                    new TermsQuery
                    {
                        Field = "Vrvirksomhed.virksomhedMetadata.nyesteHovedbranche.branchekode",
                        Terms = request.Criteria.IndustryCodes,
                        Boost = 2.0
                    },
                    new TermsQuery
                    {
                        Field = "Vrvirksomhed.virksomhedMetadata.nyesteBibranche1.branchekode",
                        Terms = request.Criteria.IndustryCodes
                    },
                    new TermsQuery
                    {
                        Field = "Vrvirksomhed.virksomhedMetadata.nyesteBibranche2.branchekode",
                        Terms = request.Criteria.IndustryCodes
                    },
                    new TermsQuery
                    {
                        Field = "Vrvirksomhed.virksomhedMetadata.nyesteBibranche3.branchekode",
                        Terms = request.Criteria.IndustryCodes
                    }
                },
                MinimumShouldMatch = 1
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryStatus(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: There are some casse where the data from cvr.dk is incorrect and the 'sammensatStatus' is different from the latest virksomhedsstatus. E.g. Cvr: 24244431

        if (request.Criteria.Statuses == null || !request.Criteria.Statuses.Any())
        {
            return null;
        }

        var queryContainers = new List<QueryContainer>();

        foreach (var status in request.Criteria.Statuses)
        {
            if (status.ToUpper() == "NORMAL")
            {
                queryContainers
                    .Add(new BoolQuery
                    {
                        Should =
                        [
                            new MatchPhraseQuery
                            {
                                Field = "Vrvirksomhed.virksomhedMetadata.sammensatStatus",
                                Query = status.Replace(" ", "")
                            },
                            new MatchPhraseQuery
                            {
                                Field = "Vrvirksomhed.virksomhedMetadata.sammensatStatus",
                                Query = "aktiv"
                            }
                        ],
                        MinimumShouldMatch = 1
                    });
            }
            else
            {
                queryContainers
                    .Add(new MatchPhraseQuery
                        {
                            Field = "Vrvirksomhed.virksomhedMetadata.sammensatStatus",
                            Query = status.Replace(" ", "")
                        }
                    );
            }
        }

        return
        [
            new BoolQuery
            {
                Should = queryContainers,
                MinimumShouldMatch = 1
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryEmployeesFrom(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.NumberOfEmployees?.From == null)
        {
            return null;
        }

        var queryContainers = new List<QueryContainer>
        {
            new ExistsQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.nyesteErstMaanedsbeskaeftigelse"
            },
            new NumericRangeQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.nyesteErstMaanedsbeskaeftigelse.antalAnsatte",
                GreaterThanOrEqualTo = request.Criteria.NumberOfEmployees.From.Value
            }
        };

        return queryContainers
            .ToArray();
    }
    private QueryContainer[] GetCompaniesSearchQueryEmployeesTo(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.NumberOfEmployees?.To == null)
        {
            return null;
        }

        return
        [
            new ExistsQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.nyesteErstMaanedsbeskaeftigelse"
            },
            new NumericRangeQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.nyesteErstMaanedsbeskaeftigelse.antalAnsatte",
                LessThanOrEqualTo = request.Criteria.NumberOfEmployees.To.Value
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryFoundedAtFrom(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.FoundedAt?.From == null)
        {
            return null;
        }

        return
        [
            new DateRangeQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.stiftelsesDato",
                GreaterThanOrEqualTo = request.Criteria.FoundedAt.From.Value.ToDateTime(new TimeOnly())
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryFoundedAtTo(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.FoundedAt?.To == null)
        {
            return null;
        }

        return
        [
            new DateRangeQuery
            {
                Field = "Vrvirksomhed.virksomhedMetadata.stiftelsesDato",
                LessThanOrEqualTo = request.Criteria.FoundedAt.To.Value.ToDateTime(new TimeOnly())
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryDissolvedAtFrom(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: There are some casse where the data from cvr.dk is incorrect, when a company has been re-opened or multiple livsforloeb entries exists

        if (request.Criteria.DissolvedAt?.From == null)
        {
            return null;
        }

        return
        [
            new DateRangeQuery
            {
                Field = "Vrvirksomhed.livsforloeb.periode.gyldigTil",
                GreaterThanOrEqualTo = request.Criteria.DissolvedAt.From.Value.ToDateTime(new TimeOnly())
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryDissolvedAtTo(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: There are some casse where the data from cvr.dk is incorrect, when a company has been re-opened or multiple livsforloeb entries exists

        if (request.Criteria.DissolvedAt?.To == null)
        {
            return null;
        }

        return
        [
            new DateRangeQuery
            {
                Field = "Vrvirksomhed.livsforloeb.periode.gyldigTil",
                LessThanOrEqualTo = request.Criteria.DissolvedAt.To.Value.ToDateTime(new TimeOnly())
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryIsProtectedFromAdvertisment(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Criteria.IsProtectedFromAdvertisment.HasValue)
        {
            return null;
        }

        return
        [
            new TermQuery
            {
                Field = "Vrvirksomhed.reklamebeskyttet",
                Value = request.Criteria.IsProtectedFromAdvertisment.Value
            }
        ];
    }
    private QueryContainer[] GetCompaniesSearchQueryIsActive(SearchCompaniesRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Criteria.IsActive.HasValue)
        {
            return null;
        }

        if (request.Criteria.IsActive.Value)
        {
            return
            [
                new NestedQuery
                {
                    Path = "Vrvirksomhed.livsforloeb",
                    Query = new BoolQuery
                    {
                        MustNot =
                        [
                            new ExistsQuery
                            {
                                Field = "Vrvirksomhed.livsforloeb.periode.gyldigTil"
                            }
                        ]
                    }
                },
                new BoolQuery
                {
                    Must =
                    [
                        new TermsQuery
                        {
                            Field = "Vrvirksomhed.virksomhedMetadata.sammensatStatus",
                            Terms = ["normal", "aktiv"]
                        }
                    ]
                }
            ];
        }

        return
        [
            new BoolQuery
            {
                MustNot =
                [
                    new TermsQuery
                    {
                        Field = "Vrvirksomhed.virksomhedMetadata.sammensatStatus",
                        Terms = ["normal", "aktiv"]
                    }
                ]
            }
        ];
    }
    private QueryContainer[] GetProductionUnitsSearchQueryName(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: The search is based on tokenized text, why searches will not always return results and why the search isn't working similar to an SQL like '{text}%'.

        if (request.Criteria.Names.Name == null)
        {
            return null;
        }

        var nameField = request.Criteria.Names.IncludeHistoricNames
            ? "VrproduktionsEnhed.navne.navn"
            : "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteNavn.navn";

        request.Criteria.Names.Name = request.Criteria.Names.Name.Trim();

        var lastIndexOfSpace = request.Criteria.Names.Name
            .LastIndexOf(' ');

        if (lastIndexOfSpace > -1)
        {
            var partOne = request.Criteria.Names.Name[..lastIndexOfSpace];
            var partTwo = request.Criteria.Names.Name[lastIndexOfSpace..];

            return
            [
                new BoolQuery
                {
                    Must =
                    [
                        new MatchPhraseQuery
                        {
                            Field = nameField,
                            Query = partOne
                        }
                    ],
                    Should = new List<QueryContainer>
                    {
                        new MatchPhraseQuery
                        {
                            Field = nameField,
                            Query = partTwo,
                            Boost = 4.0
                        },
                        new MatchPhrasePrefixQuery
                        {
                            Field = nameField,
                            Query = partTwo,
                            Boost = 2.0
                        }
                    },
                    MinimumShouldMatch = 1
                }
            ];
        }

        return
        [
            new MatchPhrasePrefixQuery
            {
                Field = nameField,
                Query = request.Criteria.Names.Name
            }
        ];
    }
    private QueryContainer[] GetProductionUnitsSearchQueryZipCodes(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.Within?.Location == null)
        {
            return null;
        }

        var postnumre = PostalCodes.GetPostalCodesWithin(request.Criteria.Within);

        return
        [
            new TermsQuery
            {
                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteBeliggenhedsadresse.postnummer",
                Terms = postnumre
            }
        ];
    }
    private QueryContainer[] GetProductionUnitsSearchQueryIndustryCodes(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Criteria.IndustryCodes.Any())
        {
            return null;
        }

        return
        [
            new BoolQuery
            {
                Should = new List<QueryContainer>
                {
                    new TermsQuery
                    {
                        Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteHovedbranche.branchekode",
                        Terms = request.Criteria.IndustryCodes,
                        Boost = 2.0
                    },
                    new TermsQuery
                    {
                        Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteBibranche1.branchekode",
                        Terms = request.Criteria.IndustryCodes
                    },
                    new TermsQuery
                    {
                        Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteBibranche2.branchekode",
                        Terms = request.Criteria.IndustryCodes
                    },
                    new TermsQuery
                    {
                        Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteBibranche3.branchekode",
                        Terms = request.Criteria.IndustryCodes
                    }
                },
                MinimumShouldMatch = 1
            }
        ];
    }
    private QueryContainer[] GetProductionUnitsSearchQueryStatus(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: There are some casse where the data from cvr.dk is incorrect and the 'sammensatStatus' equals 'Normal', while the company is Slettet.

        if (request.Criteria.Statuses == null || !request.Criteria.Statuses.Any())
        {
            return null;
        }

        var queryContainers = new List<QueryContainer>();

        foreach (var status in request.Criteria.Statuses)
        {
            if (status.ToUpper() == "NORMAL")
            {
                queryContainers
                    .Add(new BoolQuery
                    {
                        Should =
                        [
                            new MatchPhraseQuery
                            {
                                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.sammensatStatus",
                                Query = status.Replace(" ", "")
                            },
                            new MatchPhraseQuery
                            {
                                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.sammensatStatus",
                                Query = "aktiv"
                            }
                        ],
                        MinimumShouldMatch = 1
                    });
            }
            else
            {
                queryContainers
                    .Add(new MatchPhraseQuery
                    {
                        Field = "VrproduktionsEnhed.produktionsEnhedMetadata.sammensatStatus",
                        Query = status.Replace(" ", "")
                    });
            }
        }

        return
        [
            new BoolQuery
            {
                Should = queryContainers,
                MinimumShouldMatch = 1
            }
        ];
    }
    private QueryContainer[] GetProductionUnitsSearchQueryEmployeesFrom(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.NumberOfEmployees?.From == null)
        {
            return null;
        }

        var queryContainers = new List<QueryContainer>
        {
            new ExistsQuery
            {
                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteErstMaanedsbeskaeftigelse"
            },
            new NumericRangeQuery
            {
                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteErstMaanedsbeskaeftigelse.antalAnsatte",
                GreaterThanOrEqualTo = request.Criteria.NumberOfEmployees.From.Value
            }
        };

        return queryContainers
            .ToArray();
    }
    private QueryContainer[] GetProductionUnitsSearchQueryEmployeesTo(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.NumberOfEmployees?.To == null)
        {
            return null;
        }

        return
        [
            new ExistsQuery
            {
                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteErstMaanedsbeskaeftigelse"
            },
            new NumericRangeQuery
            {
                Field = "VrproduktionsEnhed.produktionsEnhedMetadata.nyesteErstMaanedsbeskaeftigelse.antalAnsatte",
                LessThanOrEqualTo = request.Criteria.NumberOfEmployees.To.Value
            }
        ];
    }
    private QueryContainer[] GetProductionUnitsSearchQueryIsActive(SearchProductionUnitsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Criteria.IsActive.HasValue)
        {
            return null;
        }

        if (request.Criteria.IsActive.Value)
        {
            return
            [
                new NestedQuery
                {
                    Path = "VrproduktionsEnhed.livsforloeb",
                    Query = new BoolQuery
                    {
                        MustNot =
                        [
                            new ExistsQuery
                            {
                                Field = "VrproduktionsEnhed.livsforloeb.periode.gyldigTil"
                            }
                        ]
                    }
                },
                new BoolQuery
                {
                    Must =
                    [
                        new TermsQuery
                        {
                            Field = "VrproduktionsEnhed.produktionsEnhedMetadata.sammensatStatus",
                            Terms = ["normal", "aktiv"]
                        }
                    ]
                }
            ];
        }

        return
        [
            new BoolQuery
            {
                MustNot =
                [
                    new TermsQuery
                    {
                        Field = "VrproduktionsEnhed.produktionsEnhedMetadata.sammensatStatus",
                        Terms = ["normal", "aktiv"]
                    }
                ]
            }
        ];
    }
    private QueryContainer[] GetPersonsAutoCompleteQueryName(AutoCompletePersonsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var queryName = this.GetPersonsSearchQueryName(new SearchPersonsRequest
        {
            Criteria =
            {
                Name = request.Name
            }
        });

        return queryName;
    }
    private QueryContainer[] GetPersonsSearchQueryName(SearchPersonsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // NOTE: The search is based on tokenized text, why searches will not always return results and why the search isn't working similar to an SQL like '{text}%'.

        if (request.Criteria.Name == null)
        {
            return null;
        }

        const string NAME_FIELD = "Vrdeltagerperson.navne.navn";

        request.Criteria.Name = request.Criteria.Name.Trim();

        var lastIndexOfSpace = request.Criteria.Name
            .LastIndexOf(' ');

        if (lastIndexOfSpace > -1)
        {
            var partOne = request.Criteria.Name[..lastIndexOfSpace];
            var partTwo = request.Criteria.Name[lastIndexOfSpace..];

            return
            [
                new BoolQuery
                {
                    Must =
                    [
                        new MatchPhraseQuery
                        {
                            Field = NAME_FIELD,
                            Query = partOne
                        }
                    ],
                    Should = new List<QueryContainer>
                    {
                        new MatchPhraseQuery
                        {
                            Field = NAME_FIELD,
                            Query = partTwo,
                            Boost = 4.0
                        },
                        new MatchPhrasePrefixQuery
                        {
                            Field = NAME_FIELD,
                            Query = partTwo,
                            Boost = 2.0
                        }
                    },
                    MinimumShouldMatch = 1
                }
            ];
        }

        return
        [
            new MatchPhrasePrefixQuery
            {
                Field = NAME_FIELD,
                Query = request.Criteria.Name
            }
        ];
    }
    private QueryContainer[] GetPersonsSearchQueryZipCodes(SearchPersonsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Criteria.Within?.Location == null)
        {
            return null;
        }

        var postnumre = PostalCodes.GetPostalCodesWithin(request.Criteria.Within);

        return
        [
            new TermsQuery
            {
                Field = "Vrdeltagerperson.deltagerpersonMetadata.nyesteBeliggenhedsadresse.postnummer",
                Terms = postnumre
            }
        ];
    }

    private CompanyDebugResult MapCompanyResult(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var registrationNumber = this.GetRegistrationNumber(virksomhed);
        var names = this.GetNames(virksomhed);
        var alternativeNameses = this.GetAlternativeNameses(virksomhed);
        var addresses = this.GetAddresseses(virksomhed);
        var phoneNumbers = this.GetPhoneNumbers(virksomhed);
        var faxNumbers = this.GetFaxNumbers(virksomhed);
        var emailAddresses = this.GetEmailAddresses(virksomhed);
        var websites = this.GetWebsites(virksomhed);
        var purposes = this.GetPurposes(virksomhed);
        var industries = this.GetIndustries(virksomhed);
        var businessTypes = this.GetBusinessTypes(virksomhed);
        var statuses = this.GetStatuses(virksomhed);
        var employment = this.GetEmployment(virksomhed);
        var registeredCapitals = this.GetRegisteredCapitals(virksomhed);
        var financialYears = this.GetFinancialYears(virksomhed);
        var antiMoneyLaundering = this.GetAntiMoneyLaundering(virksomhed);
        var auditing = this.GetAuditing(virksomhed);
        var authority = this.GetAuthority(virksomhed);
        var governance = this.GetGovernance(virksomhed);
        var ownership = this.GetOwnership(virksomhed);
        var bilawses = this.GetBilawses(virksomhed);
        var auditorRegistration = this.GetAuditorRegistration(virksomhed);
        var productionUnits = this.GetProductionUnitses(virksomhed);
        var foundedAt = this.GetFoundedAt(virksomhed);
        var effectiveStartedAt = this.GetEffectiveStartedAt(virksomhed);
        var dissolvedAt = this.GetDissolvedAt(virksomhed);
        var commercialFundApprovedAt = this.GetCommercialFundApprovedAt(virksomhed);
        var isProtectedFromAdvertisement = this.GetIsProtectedFromAdvertisement(virksomhed);
        var entityType = this.GetEntityType(virksomhed);
        var errors = this.GetErrors(virksomhed);
        var externalId = this.GetExternalId(virksomhed);
        var updateAt = this.GetUpdatedAt(virksomhed);

        var rawJson = this.GetDebugRawJson(virksomhed);

        return new CompanyDebugResult
        {
            Company = new CompanyResult
            {
                RegistrationNumber = registrationNumber,
                Names = names,
                AlternativeNames = alternativeNameses,
                Addresses = addresses,
                PhoneNumbers = phoneNumbers,
                FaxNumbers = faxNumbers,
                EmailAddresses = emailAddresses,
                Websites = websites,
                Purpose = purposes,
                Industry = industries,
                Type = businessTypes,
                Status = statuses,
                Employment = employment,
                RegisteredCapital = registeredCapitals,
                FinancialYear = financialYears,
                AntiMoneyLaundering = antiMoneyLaundering,
                Auditing = auditing,
                Authority = authority,
                Governance = governance,
                Ownership = ownership,
                Bilaws = bilawses,
                AuditorRegistration = auditorRegistration,
                ProductionUnits = productionUnits,
                FoundedAt = foundedAt,
                EffectiveStartedAt = effectiveStartedAt,
                DissolvedAt = dissolvedAt,
                CommercialFundApprovedAt = commercialFundApprovedAt,
                IsProtectedFromAdvertisement = isProtectedFromAdvertisement,
                EntityType = entityType,
                Errors = errors,
                ExternalId = externalId,
                UpdatedAt = updateAt
            },
            RawJson = rawJson
        };
    }
    private CompanySearchDebugResult MapCompanySearchResult(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var registrationNumber = this.GetRegistrationNumber(virksomhed);
        var latestName = this.GetLatestName(virksomhed);
        var address = this.GetAddress(virksomhed);
        var phoneNumber = this.GetPhoneNumber(virksomhed.TelefonNummer);
        var faxNumber = this.GetFaxNumber(virksomhed.TelefaxNummer);
        var emailAddress = this.GetEmailAddress(virksomhed.ElektroniskPost);
        var website = this.GetWebsite(virksomhed.Hjemmeside);
        var latestPurpose = this.GetLatestPurpose(virksomhed);
        var latestPrimaryIndustry = this.GetLatestPrimaryIndustry(virksomhed);
        var latestBusinessType = this.GetLatestBusinessType(virksomhed);
        var isPubliclyListed = this.GetIsPubliclyListed(virksomhed);
        var isGovernmental = this.GetIsGovernmental(virksomhed);
        var isSocialEconomic = this.GetIsSocialEconomic(virksomhed);
        var isCertifiedAuditor = this.GetIsCertifiedAuditor(virksomhed);
        var status = this.GetStatus(virksomhed);
        var latestEmployees = this.GetLatestEmployees(virksomhed);
        var isActive = this.GetStatusIsActive(virksomhed);
        var foundedAt = this.GetFoundedAt(virksomhed);
        var dissolvedAt = this.GetDissolvedAt(virksomhed);
        var isProtectedFromAdvertisement = this.GetIsProtectedFromAdvertisement(virksomhed);
        var entityType = this.GetEntityType(virksomhed);
        var externalId = this.GetExternalId(virksomhed);
        var updateAt = this.GetUpdatedAt(virksomhed);

        var rawJson = this.GetDebugRawJson(virksomhed);

        return new CompanySearchDebugResult
        {
            Company = new CompanySearchResult
            {
                RegistrationNumber = registrationNumber,
                Name = latestName,
                Address = address,
                PhoneNumber = phoneNumber,
                FaxNumber = faxNumber,
                EmailAddress = emailAddress,
                Website = website,
                Purpose = latestPurpose,
                Industry = latestPrimaryIndustry,
                Type = latestBusinessType,
                Status = status,
                Employees = latestEmployees,
                FoundedAt = foundedAt,
                DissolvedAt = dissolvedAt,
                IsProtectedFromAdvertisement = isProtectedFromAdvertisement,
                IsPubliclyListed = isPubliclyListed,
                IsGovernmental = isGovernmental,
                IsSocialEconomic = isSocialEconomic,
                IsCertifiedAuditor = isCertifiedAuditor,
                IsActive = isActive,
                EntityType = entityType,
                ExternalId = externalId,
                UpdatedAt = updateAt
            },
            RawJson = rawJson
        };
    }
    private CompanyAutoCompleteResult MapCompanyAutoCompleteResult(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var registrationNumber = this.GetRegistrationNumber(virksomhed);
        var latestName = this.GetLatestName(virksomhed);
        var businessType = this.GetLatestBusinessType(virksomhed);
        var cityName = this.GetCity(virksomhed);
        var cityPostalCode = this.GetPostalCode(virksomhed);
        var isActive = this.GetStatusIsActive(virksomhed);

        return new CompanyAutoCompleteResult
        {
            RegistrationNumber = registrationNumber,
            LatestName = latestName.Value,
            Match =
            {
                Name = latestName.Value,
                NameType = "Latest"
            },
            BusinessTypeAbbreviation = businessType.Abbreviation,
            CityName = cityName,
            CityPostalCode = cityPostalCode,
            IsActive = isActive
        };
    }
    private RegistrationTextDebugResult MapRegistrationTextResult(RegistreringsTekst registreringsTekst)
    {
        if (registreringsTekst == null)
            throw new ArgumentNullException(nameof(registreringsTekst));

        var rawJson = this.GetDebugRawJson(registreringsTekst);

        return new RegistrationTextDebugResult
        {
            RegistrationText = new RegistrationTextResult
            {
                ExternalId = registreringsTekst.OffentliggoerelseId,
                RegisteredAt = registreringsTekst.RegistreringTidsstempel,
                PublishedAt = registreringsTekst.OffentliggoerelseTidsstempel,
                RegistrationNumber = registreringsTekst.CvrNummer,
                Name = registreringsTekst.HovedNavn,
                Address = registreringsTekst.Adresse,
                PostalCode = registreringsTekst.Postnummer,
                Text = registreringsTekst.Tekst,
                UpdatedAt = registreringsTekst.SidstOpdateret,
                Statuses = registreringsTekst.VirksomhedsRegistreringStatusser
            },
            RawJson = rawJson
        };
    }
    private ProductionUnitDebugResult MapProductionUnitResult(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var productionUnitNumber = this.GetProductionUnitNumber(produktionsEnhed);
        var names = this.GetNames(produktionsEnhed);
        var addresses = this.GetAddresseses(produktionsEnhed);
        var phoneNumbers = this.GetPhoneNumbers(produktionsEnhed);
        var faxNumbers = this.GetFaxNumbers(produktionsEnhed);
        var emailAddresses = this.GetEmailAddresses(produktionsEnhed);
        var industries = this.GetIndustries(produktionsEnhed);
        var statuses = this.GetStatuses(produktionsEnhed);
        var employeeses = this.GetEmployeeses(produktionsEnhed);
        var companies = this.GetProductionUnitCompanieses(produktionsEnhed);
        var isHeaduarters = this.GetIsHeaduarters(produktionsEnhed);
        var isSupportingUnit = this.GetIsSupportingUnit(produktionsEnhed);
        var isTemporary = this.GetIsTemporary(produktionsEnhed);
        var hasConfidentiality = this.GetHasConfidentiality(produktionsEnhed);
        var foundedAt = this.GetFoundedAt(produktionsEnhed);
        var dissolvedAt = this.GetDissolvedAt(produktionsEnhed);
        var entityType = this.GetEntityType(produktionsEnhed);
        var errors = this.GetErrors(produktionsEnhed);
        var isProtectedFromAdvertisement = this.GetIsProtectedFromAdvertisement(produktionsEnhed);
        var updateAt = this.GetUpdatedAt(produktionsEnhed);

        var rawJson = this.GetDebugRawJson(produktionsEnhed);

        return new ProductionUnitDebugResult
        {
            ProductionUnit = new ProductionUnitResult
            {
                ProductionUnitNumber = productionUnitNumber,
                Names = names,
                Addresses = addresses,
                PhoneNumbers = phoneNumbers,
                FaxNumbers = faxNumbers,
                EmailAddresses = emailAddresses,
                Industry = industries,
                Status = statuses,
                Employees = employeeses,
                Companies = companies,
                IsHeaduarters = isHeaduarters,
                IsSupportingUnit = isSupportingUnit,
                IsTemporary = isTemporary,
                HasConfidentiality = hasConfidentiality,
                FoundedAt = foundedAt,
                DissolvedAt = dissolvedAt,
                EntityType = entityType,
                Errors = errors,
                IsProtectedFromAdvertisement = isProtectedFromAdvertisement,
                UpdatedAt = updateAt
            },
            RawJson = rawJson
        };
    }
    private ProductionUnitSearchDebugResult MapProductionUnitSearchResult(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var productionUnitNumber = this.GetProductionUnitNumber(produktionsEnhed);
        var name = this.GetLatestName(produktionsEnhed);
        var address = this.GetLatestAddress(produktionsEnhed);
        var phoneNumber = this.GetPhoneNumber(produktionsEnhed.TelefonNummer);
        var faxNumber = this.GetFaxNumber(produktionsEnhed.TelefaxNummer);
        var emailAddress = this.GetEmailAddress(produktionsEnhed.ElektroniskPost);
        var industry = this.GetLatestPrimaryIndustry(produktionsEnhed);
        var statuses = this.GetStatuses(produktionsEnhed);
        var employees = this.GetLatestEmployees(produktionsEnhed);
        var foundedAt = this.GetFoundedAt(produktionsEnhed);
        var dissolvedAt = this.GetDissolvedAt(produktionsEnhed);
        var entityType = this.GetEntityType(produktionsEnhed);
        var isProtectedFromAdvertisement = this.GetIsProtectedFromAdvertisement(produktionsEnhed);
        var updateAt = this.GetUpdatedAt(produktionsEnhed);

        var rawJson = this.GetDebugRawJson(produktionsEnhed);

        return new ProductionUnitSearchDebugResult
        {
            ProductionUnit = new ProductionUnitSearchResult
            {
                ProductionUnitNumber = productionUnitNumber,
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                FaxNumber = faxNumber,
                EmailAddress = emailAddress,
                Industry = industry,
                Status = statuses,
                Employees = employees,
                FoundedAt = foundedAt,
                DissolvedAt = dissolvedAt,
                EntityType = entityType,
                IsProtectedFromAdvertisement = isProtectedFromAdvertisement,
                UpdatedAt = updateAt
            },
            RawJson = rawJson
        };
    }
    private PersonDebugResult MapPersonResult(VrDeltagerPerson deltager)
    {
        if (deltager == null)
            throw new ArgumentNullException(nameof(deltager));

        var externalId = this.GetExternalId(deltager);
        var name = this.GetName(deltager);
        var addresses = this.GetAddresseses(deltager);
        var phoneNumbers = this.GetPhoneNumbers(deltager);
        var faxNumbers = this.GetFaxNumbers(deltager);
        var emailAddresses = this.GetEmailAddresses(deltager);
        var entityType = this.GetEntityType(deltager);
        var errors = this.GetErrors(deltager);
        var updatedAt = this.GetUpdatedAt(deltager);
        var title = this.GetTitle(deltager);

        var companies = deltager.VirksomhedSummariskRelation
            .Select(x =>
            {
                var registrationNumber = this.GetRegistrationNumber(x.Virksomhed);
                var innerName = this.GetName(x.Virksomhed);
                var innerExternalId = this.GetExternalId(x.Virksomhed);
                var isActive = this.GetStatusIsActive(x.Virksomhed);

                var activeRoles = this.GetPersonActiveRoles(x);

                if (!activeRoles.Any())
                {
                    return null;
                }

                return new PersonCompany
                {
                    RegistrationNumber = registrationNumber,
                    Name = innerName,
                    ExternalId = innerExternalId,
                    IsActive = isActive,
                    ActiveRoles = activeRoles
                };
            })
            .Where(x => x != null)
            .Where(x => x.IsActive);

        var rawJson = this.GetDebugRawJson(deltager);

        return new PersonDebugResult
        {
            Person = new PersonResult
            {
                ExternalId = externalId,
                Name = name,
                Title = title,
                Addresses = addresses,
                PhoneNumbers = phoneNumbers,
                FaxNumbers = faxNumbers,
                EmailAddresses = emailAddresses,
                EntityType = entityType,
                Errors = errors,
                UpdatedAt = updatedAt,
                Companies = companies
            },
            RawJson = rawJson
        };
    }
    private PersonAutoCompleteResult MapPersonAutoCompleteResult(VrDeltagerPerson deltager)
    {
        if (deltager == null)
            throw new ArgumentNullException(nameof(deltager));

        var externalId = this.GetExternalId(deltager);
        var name = this.GetName(deltager);
        var title = this.GetTitle(deltager);
        var city = this.GetCity(deltager);
        var postalCode = this.GetPostalCode(deltager);

        return new PersonAutoCompleteResult
        {
            ExternalId = externalId,
            Name = name.Value,
            Title = title,
            City = city,
            PostalCode = postalCode
        };
    }
    private PersonSearchDebugResult MapPersonSearchResult(VrDeltagerPerson deltager)
    {
        if (deltager == null)
            throw new ArgumentNullException(nameof(deltager));

        var externalId = this.GetExternalId(deltager);
        var name = this.GetName(deltager);
        var title = this.GetTitle(deltager);
        var latestAddress = this.GetLatestAddress(deltager);
        var phoneNumber = this.GetPhoneNumber(deltager.TelefonNummer);
        var faxNumber = this.GetFaxNumber(deltager.TelefaxNummer);
        var emailAddress = this.GetEmailAddress(deltager.ElektroniskPost);
        var entityType = this.GetEntityType(deltager);
        var updatedAt = this.GetUpdatedAt(deltager);
        
        var rawJson = this.GetDebugRawJson(deltager);

        return new PersonSearchDebugResult
        {
            Person = new PersonSearchResult
            {
                ExternalId = externalId,
                Name = name,
                Title = title,
                Address = latestAddress,
                PhoneNumber = phoneNumber,
                FaxNumber = faxNumber,
                EmailAddress = emailAddress,
                EntityType = entityType,
                UpdatedAt = updatedAt
            },
            RawJson = rawJson
        };
    }
    private RelationDebugResult MapRelationResult(VrDeltagerPerson deltager, bool includeHistoric)
    {
        if (deltager == null)
            throw new ArgumentNullException(nameof(deltager));

        var companies = deltager.VirksomhedSummariskRelation
            .Select(x =>
            {
                var registrationNumber = this.GetRegistrationNumber(x.Virksomhed);
                var innerName = this.GetName(x.Virksomhed);
                var ínnerEntityType = this.GetEntityType(x.Virksomhed);
                var innerExternalId = this.GetExternalId(x.Virksomhed);
                var innerUpdatedAt = this.GetUpdatedAt(x.Virksomhed);
                var foundedAt = this.GetFoundedAt(x.Virksomhed);
                var dissolvedAt = this.GetDissolvedAt(x.Virksomhed);
                var businessType = this.GetBusinessType(x.Virksomhed);
                var isActive = this.GetStatusIsActive(x.Virksomhed);
                var relationRoles = this.GetPersonRelationRoles(x);
                var activeRoles = this.GetActiveRoles(relationRoles);

                if (includeHistoric || activeRoles.Any())
                {
                    return new RelationResult
                    {
                        RegistrationNumber = registrationNumber,
                        Name = innerName,
                        EntityType = ínnerEntityType,
                        ExternalId = innerExternalId,
                        UpdatedAt = innerUpdatedAt,
                        FoundedAt = foundedAt,
                        DissolvedAt = dissolvedAt,
                        Type = businessType,
                        IsActive = isActive,
                        Roles =
                        {
                            Founder = relationRoles.Founder,
                            LegalOwner = relationRoles.LegalOwner,
                            BeneficialOwner = relationRoles.BeneficialOwner,
                            Liquidator = relationRoles.Liquidator,
                            Auditors = relationRoles.Auditors,
                            Executives = relationRoles.Executives,
                            BoardMembers = relationRoles.BoardMembers,
                            Managers = relationRoles.Managers,
                            AuthorizedSignatories = relationRoles.AuthorizedSignatories,
                            AntiMoneyLaunderingAppointees = relationRoles.AntiMoneyLaunderingAppointees,
                            SpecialFinancialParticipants = relationRoles.SpecialFinancialParticipants,
                            LiableParticipants = relationRoles.LiableParticipants,
                            AssociationRepresentatives = relationRoles.AssociationRepresentatives,
                            CertifiedAuditors = relationRoles.CertifiedAuditors
                        },
                        ActiveRoles = activeRoles
                    };
                }

                return null;
            })
            .Where(x => x != null)
            .Where(x => includeHistoric || x.IsActive);

        var rawJson = this.GetDebugRawJson(deltager);

        return new RelationDebugResult
        {
            Companies = companies,
            RawJson = rawJson
        };
    }
    private RelationDebugResult MapRelationResult(VrVirksomhed[] virksomheder, string externalId, bool includeHistoric)
    {
        if (virksomheder == null)
            throw new ArgumentNullException(nameof(virksomheder));

        if (externalId == null) 
            throw new ArgumentNullException(nameof(externalId));

        var companies = virksomheder
            .Select(x =>
            {
                var registrationNumber = this.GetRegistrationNumber(x);
                var ínnerEntityType = this.GetEntityType(x);
                var innerExternalId = this.GetExternalId(x);
                var innerUpdatedAt = this.GetUpdatedAt(x);
                var foundedAt = this.GetFoundedAt(x);
                var dissolvedAt = this.GetDissolvedAt(x);
                var innerName = this.GetLatestName(x);
                var businessType = this.GetLatestBusinessType(x);
                var isActive = this.GetStatusIsActive(x);
                var relationRoles = this.GetCompanyRelationRoles(x, externalId);
                var activeRoles = this.GetActiveRoles(relationRoles);

                if (includeHistoric || activeRoles.Any())
                {
                    return new RelationResult
                    {
                        RegistrationNumber = registrationNumber,
                        EntityType = ínnerEntityType,
                        ExternalId = innerExternalId,
                        UpdatedAt = innerUpdatedAt,
                        FoundedAt = foundedAt,
                        DissolvedAt = dissolvedAt,
                        Name = innerName,
                        Type = businessType,
                        IsActive = isActive,
                        Roles =
                        {
                            Founder = relationRoles.Founder,
                            LegalOwner = relationRoles.LegalOwner,
                            BeneficialOwner = relationRoles.BeneficialOwner,
                            Liquidator = relationRoles.Liquidator,
                            Auditors = relationRoles.Auditors,
                            Executives = relationRoles.Executives,
                            BoardMembers = relationRoles.BoardMembers,
                            Managers = relationRoles.Managers,
                            AuthorizedSignatories = relationRoles.AuthorizedSignatories,
                            AntiMoneyLaunderingAppointees = relationRoles.AntiMoneyLaunderingAppointees,
                            SpecialFinancialParticipants = relationRoles.SpecialFinancialParticipants,
                            LiableParticipants = relationRoles.LiableParticipants,
                            AssociationRepresentatives = relationRoles.AssociationRepresentatives,
                            CertifiedAuditors = relationRoles.CertifiedAuditors
                        },
                        ActiveRoles = activeRoles
                    };
                }

                return null;
            })
            .Where(x => x != null)
            .Where(x => includeHistoric || x.IsActive);

        var rawJson = this.GetDebugRawJson(virksomheder);

        return new RelationDebugResult
        {
            Companies = companies,
            RawJson = rawJson
        };
    }

    private string GetRegistrationNumber(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.CvrNummer ?? virksomhed.RegNummer
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Regnummer)
            .LastOrDefault();
    }
    private string GetRegistrationNumber(VirksomhedSummarisk virksomhedSummarisk)
    {
        if (virksomhedSummarisk == null)
            throw new ArgumentNullException(nameof(virksomhedSummarisk));

        return virksomhedSummarisk.CvrNummer;
    }
    private string GetProductionUnitNumber(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.PNummer;
    }
    private Name GetName(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Navne
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private PersonName GetName(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.Navne
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new PersonName
            {
                Value = x.Navn
            })
            .LastOrDefault();
    }
    private Name GetName(VirksomhedSummarisk deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.Navne
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Name GetName(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.Navne
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Name GetLatestName(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var name = this.GetName(virksomhed);

        if (name == null)
        {
            var nyesteNavn = virksomhed.VirksomhedMetadata.NyesteNavn;

            if (nyesteNavn == null)
            {
                var lastHistoricName = virksomhed.Navne
                    .LastOrDefault();

                if (lastHistoricName == null)
                {
                    return null;
                }

                return new Name
                {
                    Value = lastHistoricName.Navn,
                    Period =
                    {
                        From = lastHistoricName.Periode.GyldigFra,
                        To = lastHistoricName.Periode.GyldigTil
                    },
                    UpdatedAt = lastHistoricName.SidstOpdateret
                };
            }

            return new Name
            {
                Value = nyesteNavn.Navn,
                Period =
                {
                    From = nyesteNavn.Periode.GyldigFra,
                    To = nyesteNavn.Periode.GyldigTil
                },
                UpdatedAt = nyesteNavn.SidstOpdateret
            };
        }

        return name;
    }
    private Name GetLatestName(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var name = this.GetName(produktionsEnhed);

        if (name == null)
        {
            var nyesteNavn = produktionsEnhed.ProduktionsEnhedMetadata.NyesteNavn;

            if (nyesteNavn == null)
            {
                var lastHistoricName = produktionsEnhed.Navne
                    .LastOrDefault();

                if (lastHistoricName == null)
                {
                    return null;
                }

                return new Name
                {
                    Value = lastHistoricName.Navn,
                    Period =
                    {
                        From = lastHistoricName.Periode.GyldigFra,
                        To = lastHistoricName.Periode.GyldigTil
                    },
                    UpdatedAt = lastHistoricName.SidstOpdateret
                };
            }

            name = new Name
            {
                Value = nyesteNavn.Navn,
                Period =
                {
                    From = nyesteNavn.Periode.GyldigFra,
                    To = nyesteNavn.Periode.GyldigTil
                },
                UpdatedAt = nyesteNavn.SidstOpdateret
            };
        }

        return name;
    }
    private Name[] GetHistoricNames(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Navne
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Name[] GetHistoricNames(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.Navne
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Name[] GetAlternativeNames(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.BiNavne
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Name[] GetHistoricAlternativeNames(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.BiNavne
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Name
            {
                Value = x.Navn,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Names GetNames(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var name = this.GetName(virksomhed);
        var latestName = this.GetLatestName(virksomhed);
        var historicNames = this.GetHistoricNames(virksomhed);

        if (name == null && latestName == null && !historicNames.Any())
        {
            return null;
        }

        return new Names
        {
            Current = name,
            Latest = latestName,
            HistoricNames = historicNames
        };
    }
    private Names GetNames(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var name = this.GetName(produktionsEnhed);
        var latestName = this.GetLatestName(produktionsEnhed);
        var historicNames = this.GetHistoricNames(produktionsEnhed);

        if (name == null && latestName == null && !historicNames.Any())
        {
            return null;
        }

        return new Names
        {
            Current = name,
            Latest = latestName,
            HistoricNames = historicNames
        };
    }
    private string GetTitle(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.Stilling;
    }
    private string GetCity(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedMetadata.NyesteBeliggenhedsAdresse?.PostDistrikt ??
               virksomhed.BeliggenhedsAdresse.Select(x => x.PostDistrikt).LastOrDefault();
    }
    private string GetCity(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.DeltagerPersonMetadata?.NyesteBeliggenhedsAdresse?.PostDistrikt ??
               deltagerPerson.BeliggenhedsAdresse.Select(x => x.PostDistrikt).LastOrDefault();
    }
    private string GetPostalCode(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedMetadata.NyesteBeliggenhedsAdresse?.PostNummer ??
               virksomhed.BeliggenhedsAdresse.Select(x => x.PostNummer).LastOrDefault();
    }
    private string GetPostalCode(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.DeltagerPersonMetadata?.NyesteBeliggenhedsAdresse?.PostNummer ??
               deltagerPerson.BeliggenhedsAdresse.Select(x => x.PostNummer).LastOrDefault();
    }
    private Address GetAddress(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.BeliggenhedsAdresse
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(this.GetAddressOrDefault)
            .LastOrDefault();
    }
    private Address GetAddress(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.BeliggenhedsAdresse
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(this.GetAddressOrDefault)
            .LastOrDefault();
    }
    private Address GetAddress(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        var address = deltagerPerson.BeliggenhedsAdresse
            .Where(x => x.Periode.IsActive())
            .Select(this.GetAddressOrDefault)
            .LastOrDefault();

        if (address == null)
        {
            return null;
        }

        address.IsUnlisted = deltagerPerson.AdresseHemmelig;
        address.IsAddressValidationDiscontinued = deltagerPerson.AdresseOpdateringOphoert;

        return address;
    }
    private Address GetLatestAddress(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var address =
            this.GetAddress(virksomhed) ??
            this.GetAddressOrDefault(virksomhed.VirksomhedMetadata.NyesteBeliggenhedsAdresse) ??
            this.GetAddressOrDefault(virksomhed.BeliggenhedsAdresse.OrderBy(x => x.Periode.GyldigFra).LastOrDefault());

        return address;
    }
    private Address GetLatestAddress(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var address =
            this.GetAddress(produktionsEnhed) ??
            this.GetAddressOrDefault(produktionsEnhed.ProduktionsEnhedMetadata.NyesteBeliggenhedsAdresse) ??
            this.GetAddressOrDefault(produktionsEnhed.BeliggenhedsAdresse.OrderBy(x => x.Periode.GyldigFra).LastOrDefault());

        return address;
    }
    private Address GetLatestAddress(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        var address = deltagerPerson.BeliggenhedsAdresse
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(this.GetAddressOrDefault)
            .LastOrDefault();

        if (address == null)
        {
            return null;
        }

        address.IsUnlisted = deltagerPerson.AdresseHemmelig;
        address.IsAddressValidationDiscontinued = deltagerPerson.AdresseOpdateringOphoert;

        return address;
    }
    private Address[] GetHistoricAddresses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.BeliggenhedsAdresse
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(this.GetAddressOrDefault)
            .ToArray();
    }
    private Address[] GetHistoricAddresses(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.BeliggenhedsAdresse
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(this.GetAddressOrDefault)
            .ToArray();
    }
    private Address[] GetHistoricAddresses(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.BeliggenhedsAdresse
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(this.GetAddressOrDefault)
            .Select(x =>
            {
                x.IsUnlisted = deltagerPerson.AdresseHemmelig;
                x.IsAddressValidationDiscontinued = deltagerPerson.AdresseOpdateringOphoert;

                return x;
            })
            .ToArray();
    }
    private AlternativeNamesses GetAlternativeNameses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var alternativeNames = this.GetAlternativeNames(virksomhed);
        var historicAlternativeNames = this.GetHistoricAlternativeNames(virksomhed);

        if (!historicAlternativeNames.Any() && !alternativeNames.Any())
        {
            return null;
        }

        return new AlternativeNamesses
        {
            Names = alternativeNames,
            HistoricNames = historicAlternativeNames
        };
    }
    private Addresses GetAddresseses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var address = this.GetAddress(virksomhed);
        var latestAddress = this.GetLatestAddress(virksomhed);
        var historicAddresses = this.GetHistoricAddresses(virksomhed);

        if (address == null && latestAddress == null && !historicAddresses.Any())
        {
            return null;
        }

        return new Addresses
        {
            Current = address,
            Latest = latestAddress,
            HistoricAddresses = historicAddresses
        };
    }
    private Addresses GetAddresseses(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var address = this.GetAddress(produktionsEnhed);
        var latestAddress = this.GetLatestAddress(produktionsEnhed);
        var historicAddresses = this.GetHistoricAddresses(produktionsEnhed);

        if (address == null && latestAddress == null && !historicAddresses.Any())
        {
            return null;
        }

        return new Addresses
        {
            Current = address,
            Latest = latestAddress,
            HistoricAddresses = historicAddresses
        };
    }
    private Addresses GetAddresseses(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        var address = this.GetAddress(deltagerPerson);
        var latestAddress = this.GetLatestAddress(deltagerPerson);
        var historicAddresses = this.GetHistoricAddresses(deltagerPerson);

        if (address == null && latestAddress == null && !historicAddresses.Any())
        {
            return null;
        }

        return new Addresses
        {
            Current = address,
            Latest = latestAddress,
            HistoricAddresses = historicAddresses
        };
    }
    private PhoneNumber GetPhoneNumber(IEnumerable<Kontakt> kontakts)
    {
        if (kontakts == null)
            throw new ArgumentNullException(nameof(kontakts));

        return kontakts
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new PhoneNumber
            {
                Value = x.KontaktOplysning,
                IsUnlisted = x.Hemmelig,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private PhoneNumber[] GetHistoricPhoneNumbers(IEnumerable<Kontakt> kontakts)
    {
        if (kontakts == null)
            throw new ArgumentNullException(nameof(kontakts));

        return kontakts
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new PhoneNumber
            {
                Value = x.KontaktOplysning,
                IsUnlisted = x.Hemmelig,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private PhoneNumbers GetPhoneNumbers(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var phoneNumber = this.GetPhoneNumber(virksomhed.TelefonNummer);
        var historicPhoneNumbers = this.GetHistoricPhoneNumbers(virksomhed.TelefonNummer);

        if (phoneNumber == null && !historicPhoneNumbers.Any())
        {
            return null;
        }

        return new PhoneNumbers
        {
            Current = phoneNumber,
            HistoricPhoneNumbers = historicPhoneNumbers
        };
    }
    private PhoneNumbers GetPhoneNumbers(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var phoneNumber = this.GetPhoneNumber(produktionsEnhed.TelefonNummer);
        var historicPhoneNumbers = this.GetHistoricPhoneNumbers(produktionsEnhed.TelefonNummer);

        if (phoneNumber == null && !historicPhoneNumbers.Any())
        {
            return null;
        }

        return new PhoneNumbers
        {
            Current = phoneNumber,
            HistoricPhoneNumbers = historicPhoneNumbers
        };
    }
    private PhoneNumbers GetPhoneNumbers(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        var phoneNumber = this.GetPhoneNumber(deltagerPerson.TelefonNummer);
        var historicPhoneNumbers = this.GetHistoricPhoneNumbers(deltagerPerson.TelefonNummer);

        if (phoneNumber == null && !historicPhoneNumbers.Any())
        {
            return null;
        }

        return new PhoneNumbers
        {
            Current = phoneNumber,
            HistoricPhoneNumbers = historicPhoneNumbers
        };
    }
    private FaxNumber GetFaxNumber(IEnumerable<Kontakt> kontakts)
    {
        if (kontakts == null)
            throw new ArgumentNullException(nameof(kontakts));

        return kontakts
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new FaxNumber
            {
                Value = x.KontaktOplysning,
                IsUnlisted = x.Hemmelig,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private FaxNumber[] GetHistoricFaxNumbers(IEnumerable<Kontakt> kontakts)
    {
        if (kontakts == null)
            throw new ArgumentNullException(nameof(kontakts));

        return kontakts
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new FaxNumber
            {
                Value = x.KontaktOplysning,
                IsUnlisted = x.Hemmelig,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private FaxNumbers GetFaxNumbers(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var faxNumber = this.GetFaxNumber(virksomhed.TelefaxNummer);
        var historicFaxNumbers = this.GetHistoricFaxNumbers(virksomhed.TelefaxNummer);

        if (faxNumber == null && !historicFaxNumbers.Any())
        {
            return null;
        }

        return new FaxNumbers
        {
            Current = faxNumber,
            HistoricFaxNumbers = historicFaxNumbers
        };
    }
    private FaxNumbers GetFaxNumbers(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var faxNumber = this.GetFaxNumber(produktionsEnhed.TelefaxNummer);
        var historicFaxNumbers = this.GetHistoricFaxNumbers(produktionsEnhed.TelefaxNummer);

        if (faxNumber == null && !historicFaxNumbers.Any())
        {
            return null;
        }

        return new FaxNumbers
        {
            Current = faxNumber,
            HistoricFaxNumbers = historicFaxNumbers
        };
    }
    private FaxNumbers GetFaxNumbers(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        var faxNumber = this.GetFaxNumber(deltagerPerson.TelefaxNummer);
        var historicFaxNumbers = this.GetHistoricFaxNumbers(deltagerPerson.TelefaxNummer);

        if (faxNumber == null && !historicFaxNumbers.Any())
        {
            return null;
        }

        return new FaxNumbers
        {
            Current = faxNumber,
            HistoricFaxNumbers = historicFaxNumbers
        };
    }
    private EmailAddress GetEmailAddress(IEnumerable<Kontakt> kontakts)
    {
        if (kontakts == null)
            throw new ArgumentNullException(nameof(kontakts));

        return kontakts
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new EmailAddress
            {
                Value = x.KontaktOplysning,
                IsUnlisted = x.Hemmelig,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private EmailAddress[] GetHistoricEmailAddresses(IEnumerable<Kontakt> kontakts)
    {
        if (kontakts == null)
            throw new ArgumentNullException(nameof(kontakts));

        return kontakts
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new EmailAddress
            {
                Value = x.KontaktOplysning,
                IsUnlisted = x.Hemmelig,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private EmailAddresses GetEmailAddresses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var emailAddress = this.GetEmailAddress(virksomhed.ElektroniskPost);
        var historicEmailAddresses = this.GetHistoricEmailAddresses(virksomhed.ElektroniskPost);

        if (emailAddress == null && !historicEmailAddresses.Any())
        {
            return null;
        }

        return new EmailAddresses
        {
            Current = emailAddress,
            HistoricEmailAddresses = historicEmailAddresses
        };
    }
    private EmailAddresses GetEmailAddresses(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var emailAddress = this.GetEmailAddress(produktionsEnhed.ElektroniskPost);
        var historicEmailAddresses = this.GetHistoricEmailAddresses(produktionsEnhed.ElektroniskPost);

        if (emailAddress == null && !historicEmailAddresses.Any())
        {
            return null;
        }

        return new EmailAddresses
        {
            Current = emailAddress,
            HistoricEmailAddresses = historicEmailAddresses
        };
    }
    private EmailAddresses GetEmailAddresses(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        var emailAddress = this.GetEmailAddress(deltagerPerson.ElektroniskPost);
        var historicEmailAddresses = this.GetHistoricEmailAddresses(deltagerPerson.ElektroniskPost);

        if (emailAddress == null && !historicEmailAddresses.Any())
        {
            return null;
        }

        return new EmailAddresses
        {
            Current = emailAddress,
            HistoricEmailAddresses = historicEmailAddresses
        };
    }
    private Website GetWebsite(IEnumerable<Kontakt> virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Website
            {
                Value = x.KontaktOplysning,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Website[] GetHistoricWebsites(IEnumerable<Kontakt> virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Website
            {
                Value = x.KontaktOplysning,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Websites GetWebsites(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var website = this.GetWebsite(virksomhed.Hjemmeside);
        var historicWebsites = this.GetHistoricWebsites(virksomhed.Hjemmeside);

        if (website == null && !historicWebsites.Any())
        {
            return null;
        }

        return new Websites
        {
            Current = website,
            HistoricWebsites = historicWebsites
        };
    }
    private Purpose GetPurpose(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FORMÅL)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Purpose
            {
                Value = x.Vaerdi,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Purpose GetLatestPurpose(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FORMÅL)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Purpose
            {
                Value = x.Vaerdi,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Purpose GetFinancialPurpose(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var value = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FINANSIELT_FORMÅL)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .MaxBy(x => x.Periode.GyldigFra);

        if (value == null)
        {
            return null;
        }

        var success = bool.TryParse(value.Vaerdi, out _);

        if (!success)
        {
            return null;
        }

        var text = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FINANSIEL_DELTYPE)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        return new Purpose
        {
            Value = text ?? "Ja",
            Period =
            {
                From = value.Periode.GyldigFra,
                To = value.Periode.GyldigTil
            },
            UpdatedAt = value.SidstOpdateret
        };
    }
    private Purpose GetLatestFinancialPurpose(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var value = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FINANSIELT_FORMÅL)
            .SelectMany(x => x.Vaerdier)
            .MaxBy(x => x.Periode.GyldigFra);

        if (value == null)
        {
            return null;
        }

        var success = bool.TryParse(value.Vaerdi, out _);

        if (!success)
        {
            return null;
        }

        var text = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FINANSIEL_DELTYPE)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        return new Purpose
        {
            Value = text ?? "Ja",
            Period =
            {
                From = value.Periode.GyldigFra,
                To = value.Periode.GyldigTil
            },
            UpdatedAt = value.SidstOpdateret
        };
    }
    private Purpose[] GetHistoricPurposes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FORMÅL)
            .SelectMany(x => x.Vaerdier)
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Purpose
            {
                Value = x.Vaerdi,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Purposes GetPurposes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var purpose = this.GetPurpose(virksomhed);
        var latestPurpose = this.GetLatestPurpose(virksomhed);
        var historicPurposes = this.GetHistoricPurposes(virksomhed);
        var financialPurposes = this.GetFinancialPurposes(virksomhed);

        if (purpose == null && latestPurpose == null && !historicPurposes.Any() && financialPurposes == null)
        {
            return null;
        }

        return new Purposes
        {
            Current = purpose,
            Latest = latestPurpose,
            FinancialPurpose = financialPurposes,
            HistoricPurposes = historicPurposes
        };
    }
    private FinancialPurpose GetFinancialPurposes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var financialPurpose = this.GetFinancialPurpose(virksomhed);
        var latestFinancialPurpose = this.GetLatestFinancialPurpose(virksomhed);

        if (financialPurpose == null && latestFinancialPurpose == null)
        {
            return null;
        }

        return new FinancialPurpose
        {
            Current = financialPurpose,
            Latest = latestFinancialPurpose
        };
    }
    private Industry GetPrimaryIndustry(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.HovedBranche
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Industry GetPrimaryIndustry(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.HovedBranche
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private Industry GetLatestPrimaryIndustry(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var industry = this.GetPrimaryIndustry(virksomhed);

        if (industry == null)
        {
            var nyesteHovedBranche = virksomhed.VirksomhedMetadata.NyesteHovedBranche;

            if (nyesteHovedBranche == null)
            {
                var lastHistoricIndustry = virksomhed.HovedBranche
                    .LastOrDefault();

                if (lastHistoricIndustry == null)
                {
                    return null;
                }

                return new Industry
                {
                    Code = lastHistoricIndustry.BrancheKode,
                    Description = lastHistoricIndustry.BrancheTekst,
                    Period =
                    {
                        From = lastHistoricIndustry.Periode.GyldigFra,
                        To = lastHistoricIndustry.Periode.GyldigTil
                    },
                    UpdatedAt = lastHistoricIndustry.SidstOpdateret
                };
            }

            industry = new Industry
            {
                Code = nyesteHovedBranche.BrancheKode,
                Description = nyesteHovedBranche.BrancheTekst,
                Period =
                {
                    From = nyesteHovedBranche.Periode.GyldigFra,
                    To = nyesteHovedBranche.Periode.GyldigTil
                },
                UpdatedAt = nyesteHovedBranche.SidstOpdateret
            };
        }

        return industry;
    }
    private Industry GetLatestPrimaryIndustry(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var industry = this.GetPrimaryIndustry(produktionsEnhed);

        if (industry == null)
        {
            var nyesteHovedBranche = produktionsEnhed.ProduktionsEnhedMetadata.NyesteHovedBranche;

            if (nyesteHovedBranche == null)
            {
                var lastHistoricIndustry = produktionsEnhed.HovedBranche
                    .LastOrDefault();

                if (lastHistoricIndustry == null)
                {
                    return null;
                }

                return new Industry
                {
                    Code = lastHistoricIndustry.BrancheKode,
                    Description = lastHistoricIndustry.BrancheTekst,
                    Period =
                    {
                        From = lastHistoricIndustry.Periode.GyldigFra,
                        To = lastHistoricIndustry.Periode.GyldigTil
                    },
                    UpdatedAt = lastHistoricIndustry.SidstOpdateret
                };
            }

            industry = new Industry
            {
                Code = nyesteHovedBranche.BrancheKode,
                Description = nyesteHovedBranche.BrancheTekst,
                Period =
                {
                    From = nyesteHovedBranche.Periode.GyldigFra,
                    To = nyesteHovedBranche.Periode.GyldigTil
                },
                UpdatedAt = nyesteHovedBranche.SidstOpdateret
            };
        }

        return industry;
    }
    private Industry[] GetHistoricPrimaryIndustries(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.HovedBranche
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Industry[] GetHistoricPrimaryIndustries(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.HovedBranche
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Industry[] GetSecondaryIndustries(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var biBrancher = virksomhed.BiBranche1
            .Concat(virksomhed.BiBranche2)
            .Concat(virksomhed.BiBranche3);

        return biBrancher
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Industry[] GetSecondaryIndustries(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var biBrancher = produktionsEnhed.BiBranche1
            .Concat(produktionsEnhed.BiBranche2)
            .Concat(produktionsEnhed.BiBranche3);

        return biBrancher
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Industry[] GetHistoricSecondaryIndustries(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var biBrancher = virksomhed.BiBranche1
            .Concat(virksomhed.BiBranche2)
            .Concat(virksomhed.BiBranche3);

        return biBrancher
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Industry[] GetHistoricSecondaryIndustries(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var biBrancher = produktionsEnhed.BiBranche1
            .Concat(produktionsEnhed.BiBranche2)
            .Concat(produktionsEnhed.BiBranche3);

        return biBrancher
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Industry
            {
                Code = x.BrancheKode,
                Description = x.BrancheTekst,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private string GetIndustryNotes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var brancheAnsvarskode = virksomhed.BrancheAnsvarskode;
        var brancheAnsvarsKodeText = this.GetBrancheAnsvarsKodeText(virksomhed.BrancheAnsvarskode);

        if (brancheAnsvarsKodeText == null)
        {
            return null;
        }

        return $"{brancheAnsvarsKodeText} ({brancheAnsvarskode})";
    }
    private string GetIndustryNotes(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var brancheAnsvarskode = produktionsEnhed.BrancheAnsvarskode;
        var brancheAnsvarsKodeText = this.GetBrancheAnsvarsKodeText(produktionsEnhed.BrancheAnsvarskode);

        if (brancheAnsvarsKodeText == null)
        {
            return null;
        }

        return $"{brancheAnsvarsKodeText} ({brancheAnsvarskode})";
    }
    private Industries GetIndustries(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var primaryIndustry = this.GetPrimaryIndustry(virksomhed);
        var latestPrimaryIndustry = this.GetLatestPrimaryIndustry(virksomhed);
        var historicPrimaryIndustries = this.GetHistoricPrimaryIndustries(virksomhed);
        var secondaryIndustries = this.GetSecondaryIndustries(virksomhed);
        var historicSecindaryIndustries = this.GetHistoricSecondaryIndustries(virksomhed);
        var industryNotes = this.GetIndustryNotes(virksomhed);

        if (primaryIndustry == null && latestPrimaryIndustry == null && !historicPrimaryIndustries.Any() && !secondaryIndustries.Any() && !historicSecindaryIndustries.Any() && industryNotes == null)
        {
            return null;
        }

        return new Industries
        {
            Current = primaryIndustry,
            Latest = latestPrimaryIndustry,
            Notes = industryNotes,
            HistoricIndustries = historicPrimaryIndustries,
            SecondaryIndustries = secondaryIndustries,
            HistoricSecondaryIndustries = historicSecindaryIndustries
        };
    }
    private Industries GetIndustries(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var primaryIndustry = this.GetPrimaryIndustry(produktionsEnhed);
        var latestPrimaryIndustry = this.GetLatestPrimaryIndustry(produktionsEnhed);
        var historicPrimaryIndustries = this.GetHistoricPrimaryIndustries(produktionsEnhed);
        var secondaryIndustries = this.GetSecondaryIndustries(produktionsEnhed);
        var historicSecindaryIndustries = this.GetHistoricSecondaryIndustries(produktionsEnhed);
        var industryNotes = this.GetIndustryNotes(produktionsEnhed);

        if (primaryIndustry == null && latestPrimaryIndustry == null && !historicPrimaryIndustries.Any() && !secondaryIndustries.Any() && !historicSecindaryIndustries.Any() && industryNotes == null)
        {
            return null;
        }

        return new Industries
        {
            Current = primaryIndustry,
            Latest = latestPrimaryIndustry,
            Notes = industryNotes,
            HistoricIndustries = historicPrimaryIndustries,
            SecondaryIndustries = secondaryIndustries,
            HistoricSecondaryIndustries = historicSecindaryIndustries
        };
    }
    private BusinessType GetBusinessType(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedsForm
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new BusinessType
            {
                Code = x.VirksomhedsFormKode.ToString(),
                Abbreviation = x.KortBeskrivelse,
                Description = x.LangBeskrivelse,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private BusinessType GetBusinessType(VirksomhedSummarisk virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedsForm
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new BusinessType
            {
                Code = x.VirksomhedsFormKode.ToString(),
                Abbreviation = x.KortBeskrivelse,
                Description = x.LangBeskrivelse,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .LastOrDefault();
    }
    private BusinessType GetLatestBusinessType(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var businessType = this.GetBusinessType(virksomhed);

        if (businessType == null)
        {
            var nyesteVirksomhedsForm = virksomhed.VirksomhedMetadata.NyesteVirksomhedsForm;

            if (nyesteVirksomhedsForm == null)
            {
                var lastHistoricBusinessType = virksomhed.VirksomhedsForm
                    .LastOrDefault();

                if (lastHistoricBusinessType == null)
                {
                    return null;
                }

                return new BusinessType
                {
                    Code = lastHistoricBusinessType.VirksomhedsFormKode.ToString(),
                    Abbreviation = lastHistoricBusinessType.KortBeskrivelse,
                    Description = lastHistoricBusinessType.LangBeskrivelse,
                    Period =
                    {
                        From = lastHistoricBusinessType.Periode.GyldigFra,
                        To = lastHistoricBusinessType.Periode.GyldigTil
                    },
                    UpdatedAt = lastHistoricBusinessType.SidstOpdateret
                };
            }

            businessType = new BusinessType
            {
                Code = nyesteVirksomhedsForm.VirksomhedsFormKode.ToString(),
                Abbreviation = nyesteVirksomhedsForm.KortBeskrivelse,
                Description = nyesteVirksomhedsForm.LangBeskrivelse,
                Period =
                {
                    From = nyesteVirksomhedsForm.Periode.GyldigFra,
                    To = nyesteVirksomhedsForm.Periode.GyldigTil
                },
                UpdatedAt = nyesteVirksomhedsForm.SidstOpdateret
            };
        }

        return businessType;
    }
    private BusinessType[] GetHistoricBusinessTypes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedsForm
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new BusinessType
            {
                Abbreviation = x.KortBeskrivelse,
                Description = x.LangBeskrivelse,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private bool GetIsGovernmental(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var statsligVirk = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.STATSLIG_VIRK)
            .SelectMany(x => x.Vaerdier)
            .MaxBy(x => x.Periode.GyldigFra);

        if (statsligVirk == null)
        {
            return false;
        }

        var livsForloeb = virksomhed.LivsForloeb
            .LastOrDefault();

        if (livsForloeb?.Periode.IsActive() == true)
        {
            if (statsligVirk.Periode.IsActive())
            {
                return statsligVirk.Vaerdi
                    .TryParseBool();
            }

            return false;
        }

        return statsligVirk.Periode.GyldigTil == livsForloeb?.Periode.GyldigTil;
    }
    private bool GetIsPubliclyListed(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.BØRSNOTERET)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private bool GetIsSocialEconomic(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var socialØkonomiskVirksomhed = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.SOCIAL_ØKONOMISK_VIRKSOMHED)
            .SelectMany(x => x.Vaerdier)
            .MaxBy(x => x.Periode.GyldigTil);

        if (socialØkonomiskVirksomhed == null)
        {
            return false;
        }

        var livsForloeb = virksomhed.LivsForloeb
            .LastOrDefault();

        if (livsForloeb?.Periode.IsActive() == true)
        {
            if (socialØkonomiskVirksomhed.Periode.IsActive())
            {
                return socialØkonomiskVirksomhed.Vaerdi
                    .TryParseBool();
            }

            return false;
        }

        return socialØkonomiskVirksomhed.Periode.GyldigTil == livsForloeb?.Periode.GyldigTil;
    }
    private bool GetIsCertifiedAuditor(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var registreringRevisionsvirksomhedstype = virksomhed.DeltagerRelation
            .SelectMany(x => x.Organisationer
                .Where(y => y.HovedType == HovedTyper.REGISTER)
                .SelectMany(y => y.Attributter)
                .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_REVISIONSVIRKSOMHEDSTYPE)
                .SelectMany(z => z.Vaerdier))
            .MaxBy(x => x.Periode.GyldigTil);

        if (registreringRevisionsvirksomhedstype == null)
        {
            return false;
        }

        var livsForloeb = virksomhed.LivsForloeb
            .LastOrDefault();

        if (livsForloeb?.Periode.IsActive() == true)
        {
            return registreringRevisionsvirksomhedstype.Periode.IsActive();
        }

        return registreringRevisionsvirksomhedstype.Periode.GyldigTil == livsForloeb?.Periode.GyldigTil;
    }
    private BusinessTypes GetBusinessTypes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var businessType = this.GetBusinessType(virksomhed);
        var latestBusinessType = this.GetLatestBusinessType(virksomhed);
        var historicBusinessTypes = this.GetHistoricBusinessTypes(virksomhed);
        var isPubliclyListed = this.GetIsPubliclyListed(virksomhed);
        var isGovernmental = this.GetIsGovernmental(virksomhed);
        var isSocialEconomic = this.GetIsSocialEconomic(virksomhed);
        var isCertifiedAuditor = this.GetIsCertifiedAuditor(virksomhed);

        return new BusinessTypes
        {
            Current = businessType,
            Latest = latestBusinessType,
            IsPubliclyListed = isPubliclyListed,
            IsGovernmental = isGovernmental,
            IsSocialEconomic = isSocialEconomic,
            IsCertifiedAuditor = isCertifiedAuditor,
            HistoricTypes = historicBusinessTypes
        };
    }
    private Status GetStatus(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var virksomhedsStatus = virksomhed.VirksomhedsStatus
            .Where(x => x.Periode.IsActive())
            .MaxBy(x => x.Periode.GyldigFra)
            ?? virksomhed.VirksomhedsStatus
                .MaxBy(x => x.Periode.GyldigFra);

        var status = virksomhedsStatus?.Status ?? virksomhed.VirksomhedMetadata.SammensatStatus;

        status = status switch
        {
            "Aktiv" or "AKTIV" => "NORMAL",
            "UDENRETSVIRKNING" => "UDEN RETSVIRKNING",
            "UNDERFRIVILLIGLIKVIDATION" => "UNDER FRIVILLIG LIKVIDATION",
            "UNDERREKONSTRUKTION" => "UNDER REKONSTRUKTION",
            "UNDERKONKURS" => "UNDER KONKURS",
            "UNDERTVANGSOPLØSNING" => "UNDER TVANGSOPLØSNING",
            "OPLØSTEFTERKONKURS" => "OPLØST EFTER KONKURS",
            "OPLØSTEFTERFRIVILLIGLIKVIDATION" => "OPLØST EFTER FRIVILLIG LIKVIDATION",
            "OPLØSTEFTERERKLÆRING" => "OPLØST EFTER ERKLÆRING",
            "UNDERREASSUMERING" => "UNDER REASSUMERING",
            "OPLØSTEFTERFUSION" => "OPLØST EFTER FUSION",
            "OPLØSTEFTERSPALTNING" => "OPLØST EFTER SPALTNING",
            _ => status
        };

        var periode = virksomhedsStatus?.Periode;

        if (periode == null)
        {
            var periodeLivsForloeb = virksomhed.LivsForloeb
                .Select(x => x.Periode)
                .LastOrDefault();

            var from = periodeLivsForloeb?.GyldigTil ?? periodeLivsForloeb?.GyldigFra;

            if (from != null)
            {
                periode = new Periode
                {
                    GyldigFra = from
                };
            }
        }

        if (string.IsNullOrEmpty(status))
        {
            return null;
        }

        return new Status
        {
            Value = status.ToStringPretty(),
            Period = periode == null 
                ? null 
                : new Period
                {
                    From = periode.GyldigFra,
                    To = periode.GyldigTil
                },
            UpdatedAt = virksomhedsStatus?.SidstOpdateret
        };
    }
    private Status GetStatus(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var status = produktionsEnhed.ProduktionsEnhedMetadata?.SammensatStatus;

        status = status switch
        {
            "Aktiv" or "AKTIV" => "NORMAL",
            "UDENRETSVIRKNING" => "UDEN RETSVIRKNING",
            "UNDERFRIVILLIGLIKVIDATION" => "UNDER FRIVILLIG LIKVIDATION",
            "UNDERREKONSTRUKTION" => "UNDER REKONSTRUKTION",
            "UNDERKONKURS" => "UNDER KONKURS",
            "UNDERTVANGSOPLØSNING" => "UNDER TVANGSOPLØSNING",
            "OPLØSTEFTERKONKURS" => "OPLØST EFTER KONKURS",
            "OPLØSTEFTERFRIVILLIGLIKVIDATION" => "OPLØST EFTER FRIVILLIG LIKVIDATION",
            "OPLØSTEFTERERKLÆRING" => "OPLØST EFTER ERKLÆRING",
            "UNDERREASSUMERING" => "UNDER REASSUMERING",
            "OPLØSTEFTERFUSION" => "OPLØST EFTER FUSION",
            "OPLØSTEFTERSPALTNING" => "OPLØST EFTER SPALTNING",
            _ => status
        };

        if (string.IsNullOrEmpty(status))
        {
            return null;
        }

        return new Status
        {
            Value = status.ToStringPretty()
        };
    }
    private bool GetStatusIsActive(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var isActive = virksomhed.LivsForloeb
            .Any(x => x.Periode.IsActive());

        if (!isActive)
        {
            return false;
        }

        var status = this.GetStatus(virksomhed)?.Value;

        var isEnded = status != null &&
            (
                status == "Ophørt" ||
                status == "Slettet" ||
                status == "Tvangsopløst" ||
                status.StartsWith("Opløst")
            );

        return !isEnded;
    }
    private bool GetStatusIsActive(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.LivsForloeb
            .Any(x => x.Periode.IsActive());
    }
    private bool GetStatusIsActive(VirksomhedSummarisk virksomhedSummarisk)
    {
        if (virksomhedSummarisk == null)
            throw new ArgumentNullException(nameof(virksomhedSummarisk));

        return virksomhedSummarisk.LivsForloeb
            .Any(x => x.Periode.IsActive());
    }
    private Status[] GetHistoricStatusses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedsStatus
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Status
            {
                Value = x.Status,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private CreditStatus GetCreditStatus(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var status = virksomhed.Status
            .Where(x => x.Periode.IsActive())
            .MaxBy(x => x.Periode.GyldigFra);

        if (status == null)
        {
            return null;
        }

        return new CreditStatus
        {
            Code = status.KreditOplysningKode.ToString(),
            Text = this.GetKreditOplysningKodeText(status.KreditOplysningKode),
            Notes = this.GetStatusKodeText(status.StatusKode),
            Period =
            {
                From = status.Periode.GyldigFra
            },
            UpdatedAt = status.SidstOpdateret
        };
    }
    private CreditStatus[] GetHistoricCreditStatusses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Status
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new CreditStatus
            {
                Code = x.KreditOplysningKode.ToString(),
                Text = this.GetKreditOplysningKodeText(x.KreditOplysningKode),
                Notes = this.GetStatusKodeText(x.StatusKode),
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private Statuses GetStatuses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var status = this.GetStatus(virksomhed);
        var isActive = this.GetStatusIsActive(virksomhed);
        var historicStatusses = this.GetHistoricStatusses(virksomhed);
        var creditStatuses = this.GetCreditStatuses(virksomhed);

        return new Statuses
        {
            Current = status,
            IsActive = isActive,
            CreditStatus = creditStatuses,
            HistoricStatuses = historicStatusses
        };
    }
    private StatusesSimple GetStatuses(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var status = this.GetStatus(produktionsEnhed);
        var isActive = this.GetStatusIsActive(produktionsEnhed);

        return new StatusesSimple
        {
            Current = status,
            IsActive = isActive
        };
    }
    private CreditStatuses GetCreditStatuses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var creditStatus = this.GetCreditStatus(virksomhed);
        var historicCreditStatusses = this.GetHistoricCreditStatusses(virksomhed);

        if (creditStatus == null && !historicCreditStatusses.Any())
        {
            return null;
        }

        return new CreditStatuses
        {
            Current = creditStatus,
            HistoricStatuses = historicCreditStatusses
        };
    }
    private Employees GetEmployees(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var isActive = this.GetStatusIsActive(virksomhed);

        if (!isActive)
        {
            return null;
        }

        return this.GetEmployeess(virksomhed)
            .LastOrDefault();
    }
    private Employees GetEmployees(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var isActive = this.GetStatusIsActive(produktionsEnhed);

        if (!isActive)
        {
            return null;
        }

        return this.GetEmployeess(produktionsEnhed)
            .LastOrDefault();
    }
    private Employees GetLatestEmployees(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return this.GetEmployeess(virksomhed)
            .LastOrDefault();
    }
    private Employees GetLatestEmployees(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return this.GetEmployeess(produktionsEnhed)
            .LastOrDefault();
    }
    private Employees[] GetEmployeess(VrVirksomhed virksomhed)
    {
        if (virksomhed == null) 
            throw new ArgumentNullException(nameof(virksomhed));
        
        var employees = new List<Employees>();

        var erstMaanedsBeskaeftigelser = virksomhed.ErstMaanedsBeskaeftigelse
            .Select(x =>
            {
                var day = DateTime.DaysInMonth(x.Aar, x.Maaned);

                return new Employees
                {
                    NumberOfEmployees = x.AntalAnsatte,
                    NumberOfFulltimePositions = x.AntalAarsvaerk,
                    Period = new Period
                    {
                        From = new DateOnly(x.Aar, x.Maaned, 1),
                        To = new DateOnly(x.Aar, x.Maaned, day)
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            });

        employees
            .AddRange(erstMaanedsBeskaeftigelser);

        var minEmployee = employees
            .MinBy(x => x.Period.To);

        var maanedsBeskaeftigelser = virksomhed.MaanedsBeskaeftigelse
            .Select(x =>
            {
                var day = DateTime.DaysInMonth(x.Aar, x.Maaned);

                return new Employees
                {
                    NumberOfEmployees = x.AntalAnsatte,
                    NumberOfFulltimePositions = x.AntalAarsvaerk,
                    Period = new Period
                    {
                        From = new DateOnly(x.Aar, x.Maaned, 1),
                        To = new DateOnly(x.Aar, x.Maaned, day)
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            })
            .Where(x => minEmployee == null || x.Period.From < minEmployee.Period.From)
            .ToArray();

        employees
            .AddRange(maanedsBeskaeftigelser);

        minEmployee = employees
            .MinBy(x => x.Period.To);

        var kvartalsBeskaeftigelser = virksomhed.KvartalsBeskaeftigelse
            .Select(x =>
            {
                var maanedBegin = (x.Kvartal - 1) * 3 + 1;
                var maanedEnd = maanedBegin + 2;
                var day = DateTime.DaysInMonth(x.Aar, maanedEnd);

                return new Employees
                {
                    NumberOfEmployees = x.AntalAnsatte,
                    NumberOfFulltimePositions = x.AntalAarsvaerk,
                    Period = new Period
                    {
                        From = new DateOnly(x.Aar, maanedBegin, 1),
                        To = new DateOnly(x.Aar, maanedEnd, day)
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            })
            .Where(x => minEmployee == null || x.Period.From < minEmployee.Period.From)
            .ToArray();

        employees
            .AddRange(kvartalsBeskaeftigelser);

        minEmployee = employees
            .MinBy(x => x.Period.To);

        var aarsBeskaeftigelser = virksomhed.AarsBeskaeftigelse
            .Select(x => new Employees
            {
                NumberOfEmployees = x.AntalAnsatte,
                NumberOfFulltimePositions = x.AntalAarsvaerk,
                Period = new Period
                {
                    From = new DateOnly(x.Aar, 1, 1),
                    To = new DateOnly(x.Aar, 12, 31)
                },
                UpdatedAt = x.SidstOpdateret
            })
            .Where(x => minEmployee == null || x.Period.To < minEmployee.Period.To)
            .ToArray();

        employees
            .AddRange(aarsBeskaeftigelser);

        return employees
            .OrderBy(x => x.Period.From)
            .ToArray();
    }
    private Employees[] GetEmployeess(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var employees = new List<Employees>();

        var erstMaanedsBeskaeftigelser = produktionsEnhed.ErstMaanedsBeskaeftigelse
            .Select(x =>
            {
                var day = DateTime.DaysInMonth(x.Aar, x.Maaned);

                return new Employees
                {
                    NumberOfEmployees = x.AntalAnsatte,
                    NumberOfFulltimePositions = x.AntalAarsvaerk,
                    Period = new Period
                    {
                        From = new DateOnly(x.Aar, x.Maaned, 1),
                        To = new DateOnly(x.Aar, x.Maaned, day)
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            });

        employees
            .AddRange(erstMaanedsBeskaeftigelser);

        var minEmployee = employees
            .MinBy(x => x.Period.To);

        var maanedsBeskaeftigelser = produktionsEnhed.MaanedsBeskaeftigelse
            .Select(x =>
            {
                var day = DateTime.DaysInMonth(x.Aar, x.Maaned);

                return new Employees
                {
                    NumberOfEmployees = x.AntalAnsatte,
                    NumberOfFulltimePositions = x.AntalAarsvaerk,
                    Period = new Period
                    {
                        From = new DateOnly(x.Aar, x.Maaned, 1),
                        To = new DateOnly(x.Aar, x.Maaned, day)
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            })
            .Where(x => minEmployee == null || x.Period.From < minEmployee.Period.From)
            .ToArray();

        employees
            .AddRange(maanedsBeskaeftigelser);

        minEmployee = employees
            .MinBy(x => x.Period.To);

        var kvartalsBeskaeftigelser = produktionsEnhed.KvartalsBeskaeftigelse
            .Select(x =>
            {
                var maanedBegin = (x.Kvartal - 1) * 3 + 1;
                var maanedEnd = maanedBegin + 2;
                var day = DateTime.DaysInMonth(x.Aar, maanedEnd);

                return new Employees
                {
                    NumberOfEmployees = x.AntalAnsatte,
                    NumberOfFulltimePositions = x.AntalAarsvaerk,
                    Period = new Period
                    {
                        From = new DateOnly(x.Aar, maanedBegin, 1),
                        To = new DateOnly(x.Aar, maanedEnd, day)
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            })
            .Where(x => minEmployee == null || x.Period.From < minEmployee.Period.From)
            .ToArray();

        employees
            .AddRange(kvartalsBeskaeftigelser);

        minEmployee = employees
            .MinBy(x => x.Period.To);

        var aarsBeskaeftigelser = produktionsEnhed.AarsBeskaeftigelse
            .Select(x => new Employees
            {
                NumberOfEmployees = x.AntalAnsatte,
                NumberOfFulltimePositions = x.AntalAarsvaerk,
                Period = new Period
                {
                    From = new DateOnly(x.Aar, 1, 1),
                    To = new DateOnly(x.Aar, 12, 31)
                },
                UpdatedAt = x.SidstOpdateret
            })
            .Where(x => minEmployee == null || x.Period.To < minEmployee.Period.To)
            .ToArray();

        employees
            .AddRange(aarsBeskaeftigelser);

        return employees
            .OrderBy(x => x.Period.From)
            .ToArray();
    }
    private Employees[] GetHistoricEmployees(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var isActive = this.GetStatusIsActive(virksomhed);

        var skipLast = isActive ? 1 : 0;

        return this.GetEmployeess(virksomhed)
            .SkipLast(skipLast)
            .ToArray();
    }
    private Employees[] GetHistoricEmployees(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var isActive = this.GetStatusIsActive(produktionsEnhed);

        var skipLast = isActive ? 1 : 0;

        return this.GetEmployeess(produktionsEnhed)
            .SkipLast(skipLast)
            .ToArray();
    }
    private RelationManagers GetManagers(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.DAGLIG_LEDELSE,
            AttributVaerdier.UDDELER,
            AttributVaerdier.TILSYNSRÅD,
            AttributVaerdier.LEDELSE,
            AttributVaerdier.ANDET_LEDELSESORGAN,
            AttributVaerdier.KONTROLRÅD,
            AttributVaerdier.DRIFTSLEDER,
            AttributVaerdier.ANDEN_DAGLIG_LEDELSE,
            AttributVaerdier.PROKURIST,
            AttributVaerdier.ØVERSTE_LEDELSESORGAN,
            AttributVaerdier.BESTYRER,
            AttributVaerdier.LEDELSESKOMITEE,
            AttributVaerdier.LEDER_FOR_JOINT_VENTURE,
            AttributVaerdier.CITYCHEF,
            AttributVaerdier.OVERORDNET_LEDELSE,
            AttributVaerdier.DAGLIG_LEDELSE_OG_ADM,
            AttributVaerdier.SKOLELEDER,
            AttributVaerdier.ADMINISTRATIV_LEDER,
            AttributVaerdier.KÆDECHEF,
            AttributVaerdier.LEDELSESORGAN,
            AttributVaerdier.MEJERIBESTYRER,
            AttributVaerdier.STATIONSLEDER,
            AttributVaerdier.DAGLIG_LEDER,
            AttributVaerdier.CENTERLEDER,
            AttributVaerdier.INTERESSENTSKABSLEDELSE,
            AttributVaerdier.ANSVARLIG_LEDELSE,
            AttributVaerdier.TURIST_OG_ERHVERVSCHEF,
            AttributVaerdier.BUTIKSBESTYRER,
            AttributVaerdier.LEDER,
            AttributVaerdier.ANSVARLIG_LEDER,
            AttributVaerdier.LEDELSESUDVALG,
            AttributVaerdier.MANAGER,
            AttributVaerdier.PROJEKTCHEF,
            AttributVaerdier.UDVALG,
            AttributVaerdier.KASSERER,
            AttributVaerdier.GENERALAGENT,
            AttributVaerdier.KOMMANDITISTREPRÆSENTANT,
            AttributVaerdier.FORRETNINGSBESTYRELSE,
            AttributVaerdier.ANSVARLIG_DELTAGER,
            AttributVaerdier.FORRETNINGSUDVALG,
            AttributVaerdier.KOMMITTERET,
            AttributVaerdier.STYREGRUPPE,
            AttributVaerdier.PRÆSIDIET,
            AttributVaerdier.STYRELSE,
            AttributVaerdier.REPRÆSENTANTSKAB,
            AttributVaerdier.REKONSTRUKTØRER,
            AttributVaerdier.REVISIONSVIRKSOMHEDLEDELSE,
            AttributVaerdier.ADMINISTRATOR,
            AttributVaerdier.ADMINISTRATIONSORGAN,
            AttributVaerdier.ADMINISTRATIONSSELSKAB,
            AttributVaerdier.ADMINISTRATION,
            AttributVaerdier.SELSKABSADMINISTRATION
        };

        var managers = this.GetRelations<Manager>(virksomhedSummariskRelation, HovedTyper.LEDELSESORGAN, attributVærdier);

        if (!managers.Any())
        {
            return null;
        }

        var current = managers
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationManager
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = managers
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationManager
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationManagers
        {
            Current = current,
            HistoricManagers = historic
        };
    }
    private RelationManagers GetManagers(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.DAGLIG_LEDELSE,
            AttributVaerdier.UDDELER,
            AttributVaerdier.TILSYNSRÅD,
            AttributVaerdier.LEDELSE,
            AttributVaerdier.ANDET_LEDELSESORGAN,
            AttributVaerdier.KONTROLRÅD,
            AttributVaerdier.DRIFTSLEDER,
            AttributVaerdier.ANDEN_DAGLIG_LEDELSE,
            AttributVaerdier.PROKURIST,
            AttributVaerdier.ØVERSTE_LEDELSESORGAN,
            AttributVaerdier.BESTYRER,
            AttributVaerdier.LEDELSESKOMITEE,
            AttributVaerdier.LEDER_FOR_JOINT_VENTURE,
            AttributVaerdier.CITYCHEF,
            AttributVaerdier.OVERORDNET_LEDELSE,
            AttributVaerdier.DAGLIG_LEDELSE_OG_ADM,
            AttributVaerdier.SKOLELEDER,
            AttributVaerdier.ADMINISTRATIV_LEDER,
            AttributVaerdier.KÆDECHEF,
            AttributVaerdier.LEDELSESORGAN,
            AttributVaerdier.MEJERIBESTYRER,
            AttributVaerdier.STATIONSLEDER,
            AttributVaerdier.DAGLIG_LEDER,
            AttributVaerdier.CENTERLEDER,
            AttributVaerdier.INTERESSENTSKABSLEDELSE,
            AttributVaerdier.ANSVARLIG_LEDELSE,
            AttributVaerdier.TURIST_OG_ERHVERVSCHEF,
            AttributVaerdier.BUTIKSBESTYRER,
            AttributVaerdier.LEDER,
            AttributVaerdier.ANSVARLIG_LEDER,
            AttributVaerdier.LEDELSESUDVALG,
            AttributVaerdier.MANAGER,
            AttributVaerdier.PROJEKTCHEF,
            AttributVaerdier.UDVALG,
            AttributVaerdier.KASSERER,
            AttributVaerdier.GENERALAGENT,
            AttributVaerdier.KOMMANDITISTREPRÆSENTANT,
            AttributVaerdier.FORRETNINGSBESTYRELSE,
            AttributVaerdier.ANSVARLIG_DELTAGER,
            AttributVaerdier.FORRETNINGSUDVALG,
            AttributVaerdier.KOMMITTERET,
            AttributVaerdier.STYREGRUPPE,
            AttributVaerdier.PRÆSIDIET,
            AttributVaerdier.STYRELSE,
            AttributVaerdier.REPRÆSENTANTSKAB,
            AttributVaerdier.REKONSTRUKTØRER,
            AttributVaerdier.REVISIONSVIRKSOMHEDLEDELSE,
            AttributVaerdier.ADMINISTRATOR,
            AttributVaerdier.ADMINISTRATIONSORGAN,
            AttributVaerdier.ADMINISTRATIONSSELSKAB,
            AttributVaerdier.ADMINISTRATION,
            AttributVaerdier.SELSKABSADMINISTRATION
        };

        var managers = this.GetRelations<Manager>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!managers.Any())
        {
            return null;
        }

        var current = managers
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationManager
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = managers
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationManager
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationManagers
        {
            Current = current,
            HistoricManagers = historic
        };
    }
    private Managers GetManagers(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.DAGLIG_LEDELSE,
            AttributVaerdier.UDDELER,
            AttributVaerdier.TILSYNSRÅD,
            AttributVaerdier.LEDELSE,
            AttributVaerdier.ANDET_LEDELSESORGAN,
            AttributVaerdier.KONTROLRÅD,
            AttributVaerdier.DRIFTSLEDER,
            AttributVaerdier.ANDEN_DAGLIG_LEDELSE,
            AttributVaerdier.PROKURIST,
            AttributVaerdier.ØVERSTE_LEDELSESORGAN,
            AttributVaerdier.BESTYRER,
            AttributVaerdier.LEDELSESKOMITEE,
            AttributVaerdier.LEDER_FOR_JOINT_VENTURE,
            AttributVaerdier.CITYCHEF,
            AttributVaerdier.OVERORDNET_LEDELSE,
            AttributVaerdier.DAGLIG_LEDELSE_OG_ADM,
            AttributVaerdier.SKOLELEDER,
            AttributVaerdier.ADMINISTRATIV_LEDER,
            AttributVaerdier.KÆDECHEF,
            AttributVaerdier.LEDELSESORGAN,
            AttributVaerdier.MEJERIBESTYRER,
            AttributVaerdier.STATIONSLEDER,
            AttributVaerdier.DAGLIG_LEDER,
            AttributVaerdier.CENTERLEDER,
            AttributVaerdier.INTERESSENTSKABSLEDELSE,
            AttributVaerdier.ANSVARLIG_LEDELSE,
            AttributVaerdier.TURIST_OG_ERHVERVSCHEF,
            AttributVaerdier.BUTIKSBESTYRER,
            AttributVaerdier.LEDER,
            AttributVaerdier.ANSVARLIG_LEDER,
            AttributVaerdier.LEDELSESUDVALG,
            AttributVaerdier.MANAGER,
            AttributVaerdier.PROJEKTCHEF,
            AttributVaerdier.UDVALG,
            AttributVaerdier.KASSERER,
            AttributVaerdier.GENERALAGENT,
            AttributVaerdier.KOMMANDITISTREPRÆSENTANT,
            AttributVaerdier.FORRETNINGSBESTYRELSE,
            AttributVaerdier.ANSVARLIG_DELTAGER,
            AttributVaerdier.FORRETNINGSUDVALG,
            AttributVaerdier.KOMMITTERET,
            AttributVaerdier.STYREGRUPPE,
            AttributVaerdier.PRÆSIDIET,
            AttributVaerdier.STYRELSE,
            AttributVaerdier.REPRÆSENTANTSKAB,
            AttributVaerdier.REKONSTRUKTØRER,
            AttributVaerdier.REVISIONSVIRKSOMHEDLEDELSE,
            AttributVaerdier.ADMINISTRATOR,
            AttributVaerdier.ADMINISTRATIONSORGAN,
            AttributVaerdier.ADMINISTRATIONSSELSKAB,
            AttributVaerdier.ADMINISTRATION,
            AttributVaerdier.SELSKABSADMINISTRATION
        };

        var managers = this.GetRelations<Manager>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier);

        if (!managers.Any())
        {
            return null;
        }

        var current = managers
            .Where(x => x.Period.IsValid());

        var historic = managers
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To)
            .ToArray();

        return new Managers
        {
            Current = current,
            HistoricManagers = historic
        };
    }
    private Employment GetEmployment(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var employeeses = this.GetEmployeeses(virksomhed);
        var management = this.GetManagers(virksomhed);

        if (employeeses == null && management == null)
        {
            return null;
        }

        return new Employment
        {
            Employees = employeeses,
            Managers = management
        };
    }
    private Employeeses GetEmployeeses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var employees = this.GetEmployees(virksomhed);
        var latestEmployees = this.GetLatestEmployees(virksomhed);
        var historicEmployees = this.GetHistoricEmployees(virksomhed);

        if (employees == null && latestEmployees == null && !historicEmployees.Any())
        {
            return null;
        }

        return new Employeeses
        {
            Current = employees,
            Latest = latestEmployees,
            HistoricEmployees = historicEmployees
        };
    }
    private Employeeses GetEmployeeses(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var employees = this.GetEmployees(produktionsEnhed);
        var latestEmployees = this.GetLatestEmployees(produktionsEnhed);
        var historicEmployees = this.GetHistoricEmployees(produktionsEnhed);

        if (employees == null && latestEmployees == null && !historicEmployees.Any())
        {
            return null;
        }

        return new Employeeses
        {
            Current = employees,
            Latest = latestEmployees,
            HistoricEmployees = historicEmployees
        };
    }
    private RegisteredCapital GetRegisteredCapital(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var capital = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.KAPITAL)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .LastOrDefault(x => x.Periode.IsActive());

        if (capital == null)
        {
            return null;
        }

        var currency = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.KAPITALVALUTA)
            .SelectMany(x => x.Vaerdier)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        return new RegisteredCapital
        {
            Value = capital.Vaerdi?.TryParseDouble(DanishCvrService.numberFormatInfo),
            Currency = currency,
            Period =
            {
                From = capital.Periode.GyldigFra
            },
            UpdatedAt = capital.SidstOpdateret
        };
    }
    private bool GetRegisteredCapitalIsPartiallyPaid(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.KAPITAL_DELVIST)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private RegisteredCapital[] GetHistoricRegisteredCapitals(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var capitals = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.KAPITAL)
            .SelectMany(x => x.Vaerdier)
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra);

        var currencies = virksomhed.Attributter
            .Where(y => y.Type == AttributTyper.KAPITALVALUTA)
            .SelectMany(y => y.Vaerdier)
            .ToArray();

        return capitals
            .Select(x =>
            {
                var currency = currencies.LastOrDefault(y => y.Periode.GyldigFra >= x.Periode.GyldigFra) ??
                               currencies.LastOrDefault();

                return new RegisteredCapital
                {
                    Value = x.Vaerdi?.TryParseDouble(DanishCvrService.numberFormatInfo),
                    Currency = currency?.Vaerdi,
                    Period =
                    {
                        From = x.Periode.GyldigFra,
                        To = x.Periode.GyldigTil
                    }
                };
            })
            .ToArray();
    }
    private RegisteredCapitals GetRegisteredCapitals(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var registeredCapital = this.GetRegisteredCapital(virksomhed);
        var historicRegisteredCapitals = this.GetHistoricRegisteredCapitals(virksomhed);
        var isPartiallyPaid = this.GetRegisteredCapitalIsPartiallyPaid(virksomhed);

        if (registeredCapital == null && !historicRegisteredCapitals.Any() && !isPartiallyPaid)
        {
            return null;
        }

        return new RegisteredCapitals
        {
            Current = registeredCapital,
            HistoricRegisteredCapitals = historicRegisteredCapitals,
            IsPartiallyPaid = isPartiallyPaid
        };
    }
    private FinancialYear GetFinancialYear(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var regnskabsårStart = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_START)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .LastOrDefault();

        var regnskabsårSlut = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_SLUT)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .LastOrDefault();

        if (regnskabsårStart == null || regnskabsårSlut == null)
        {
            return null;
        }

        var finanicalYearBegin = regnskabsårStart.Vaerdi
            .GetYearAndMonth();

        var finanicalYearEnd = regnskabsårSlut.Vaerdi
            .GetYearAndMonth();

        return new FinancialYear
        {
            Begin = finanicalYearBegin,
            End = finanicalYearEnd,
            Period =
            {
                From = regnskabsårStart.Periode.GyldigFra
            },
            UpdatedAt = regnskabsårStart.SidstOpdateret
        };
    }
    private FinancialYear GetLatestFinancialYear(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var regnskabsårStart = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_START)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .LastOrDefault();

        var regnskabsårSlut = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_SLUT)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .LastOrDefault();

        if (regnskabsårStart == null || regnskabsårSlut == null)
        {
            return null;
        }

        var finanicalYearBegin = regnskabsårStart.Vaerdi
            .GetYearAndMonth();

        var finanicalYearEnd = regnskabsårSlut.Vaerdi
            .GetYearAndMonth();

        return new FinancialYear
        {
            Begin = finanicalYearBegin,
            End = finanicalYearEnd,
            Period =
            {
                From = regnskabsårStart.Periode.GyldigFra,
                To = regnskabsårStart.Periode.GyldigTil
            },
            UpdatedAt = regnskabsårSlut.SidstOpdateret
        };
    }
    private Period GetFirstFinancialYear(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var firstStartVaerdiers = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FØRSTE_REGNSKABSPERIODE_START)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var from =
            firstStartVaerdiers
                .Where(x => x.Periode.IsActive())
                .Select(x => x.Vaerdi.TryParseDateOnly())
                .LastOrDefault() ??
            firstStartVaerdiers
                .Select(x => x.Vaerdi.TryParseDateOnly())
                .LastOrDefault();

        if (from == null)
        {
            return null;
        }

        var firstSlutVaerdiers = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.FØRSTE_REGNSKABSPERIODE_SLUT)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var to =
            firstSlutVaerdiers
                .Where(x => x.Periode.IsActive())
                .Select(x => x.Vaerdi.TryParseDateOnly())
                .LastOrDefault() ??
            firstSlutVaerdiers
                .Select(x => x.Vaerdi.TryParseDateOnly())
                .LastOrDefault();

        return new Period
        {
            From = from,
            To = to
        };
    }
    private Period GetFinancialOngoingTransitionPeriod(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var transitionPeriodStart = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.OMLÆGNINGSPERIODE_START)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi.TryParseDateOnly())
            .LastOrDefault();

        var transitionPeriodEnd = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.OMLÆGNINGSPERIODE_SLUT)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi.TryParseDateOnly())
            .LastOrDefault();

        return transitionPeriodStart == null
            ? null
            : new Period
            {
                From = transitionPeriodStart,
                To = transitionPeriodEnd
            };
    }
    private string GetFinancialYearNotes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_FRITEKST)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi)
            .LastOrDefault();
    }
    private FinancialYear[] GetHistoricFinancialYears(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var regnskabsårStarts = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_START)
            .SelectMany(x => x.Vaerdier)
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var regnskabsårSluts = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REGNSKABSÅR_SLUT)
            .SelectMany(x => x.Vaerdier)
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi.GetYearAndMonth())
            .ToArray();

        return regnskabsårStarts
            .Select((x, y) =>
            {
                var monthAndDaySlut = regnskabsårSluts
                    .ElementAtOrDefault(y);

                if (monthAndDaySlut == null)
                {
                    var regnskabsårSlutsFixed = virksomhed.Attributter
                        .Where(z => z.Type == AttributTyper.REGNSKABSÅR_SLUT)
                        .SelectMany(z => z.Vaerdier)
                        .OrderBy(z => z.Periode.GyldigFra)
                        .Select(z => z.Vaerdi.GetYearAndMonth())
                        .ToArray();

                    monthAndDaySlut = regnskabsårSlutsFixed
                        .ElementAtOrDefault(y);
                }

                return new FinancialYear
                {
                    Begin = x.Vaerdi.GetYearAndMonth(),
                    End = monthAndDaySlut,
                    Period =
                    {
                        From = x.Periode.GyldigFra,
                        To = x.Periode.GyldigTil
                    },
                    UpdatedAt = x.SidstOpdateret
                };
            })
            .ToArray();
    }
    private FinancialYears GetFinancialYears(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var financialYear = this.GetFinancialYear(virksomhed);
        var latestFinancialYear = this.GetLatestFinancialYear(virksomhed);
        var historicFinancialYears = this.GetHistoricFinancialYears(virksomhed);
        var firstFinancialYear = this.GetFirstFinancialYear(virksomhed);
        var financialOngoingTransitionPeriod = this.GetFinancialOngoingTransitionPeriod(virksomhed);
        var financialYearNotes = this.GetFinancialYearNotes(virksomhed);

        if (financialYear == null && latestFinancialYear == null && !historicFinancialYears.Any() && firstFinancialYear == null && financialOngoingTransitionPeriod == null && financialYearNotes == null)
        {
            return null;
        }

        return new FinancialYears
        {
            Current = financialYear,
            Latest = latestFinancialYear,
            FirstFinancialYear = firstFinancialYear,
            OngoingTransitionPeriod = financialOngoingTransitionPeriod,
            Notes = financialYearNotes,
            HistoricFinancialYears = historicFinancialYears
        };
    }
    private string GetAntiMoneyLaunderingText(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.DeltagerRelation
            .SelectMany(x => x.Organisationer
                .Where(y => y.HovedType == HovedTyper.HVIDVASK)
                .SelectMany(y => y.OrganisationsNavn))
            .Select(x => x.Navn)
            .LastOrDefault();
    }
    private string[] GetAntiMoneyLaunderingActivities(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.DeltagerRelation
            .SelectMany(x => x.Organisationer
                .Where(y => y.HovedType == HovedTyper.HVIDVASK)
                .SelectMany(y => y.Attributter)
                .Where(y => y.Type == AttributTyper.HVIDVASK_ERHVERVSAKTIVITET)
                .SelectMany(z => z.Vaerdier
                    .Select(a => a.Vaerdi.ToStringPretty())))
            .Distinct()
            .ToArray();
    }
    private bool GetIsSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return !virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.OMFATTET_AF_LOV_OM_HVIDVASK_OG_TERRORFINANSIERING)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .Select(x => x.Vaerdi != "IKKE_OMFATTET")
            .LastOrDefault();
    }
    private RelationAntiMoneyLaunderingAppointees GetAntiMoneyLaunderingAppointees(VirksomhedSummariskRelation virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.HVIDVASK
        };

        var antiMoneyLaunderingAppointees = this.GetRelations<AntiMoneyLaunderingAppointee>(virksomhed, HovedTyper.HVIDVASK, attributVærdier);

        if (!antiMoneyLaunderingAppointees.Any())
        {
            return null;
        }

        var current = antiMoneyLaunderingAppointees
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationAntiMoneyLaunderingAppointee
            {
                Title = x.Title,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = antiMoneyLaunderingAppointees
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAntiMoneyLaunderingAppointee
            {
                Title = x.Title,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAntiMoneyLaunderingAppointees
        {
            Current = current,
            HistoricAntiMoneyLaunderingAppointees = historic
        };
    }
    private RelationAntiMoneyLaunderingAppointees GetAntiMoneyLaunderingAppointees(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.HVIDVASK
        };

        var antiMoneyLaunderingAppointees = this.GetRelations<AntiMoneyLaunderingAppointee>(virksomhed, HovedTyper.HVIDVASK, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!antiMoneyLaunderingAppointees.Any())
        {
            return null;
        }

        var current = antiMoneyLaunderingAppointees
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationAntiMoneyLaunderingAppointee
            {
                Title = x.Title,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = antiMoneyLaunderingAppointees
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAntiMoneyLaunderingAppointee
            {
                Title = x.Title,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAntiMoneyLaunderingAppointees
        {
            Current = current,
            HistoricAntiMoneyLaunderingAppointees = historic
        };
    }
    private AntiMoneyLaunderingAppointees GetAntiMoneyLaunderingAppointees(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.HVIDVASK
        };

        var antiMoneyLaunderingAppointees = this.GetRelations<AntiMoneyLaunderingAppointee>(virksomhed, HovedTyper.HVIDVASK, attributVærdier);

        if (!antiMoneyLaunderingAppointees.Any())
        {
            return null;
        }

        var current = antiMoneyLaunderingAppointees
            .LastOrDefault(x => x.Period.IsValid());

        var historic = antiMoneyLaunderingAppointees
            .Where(x => !x.Period.IsValid());

        return new AntiMoneyLaunderingAppointees
        {
            Current = current,
            HistoricAntiMoneyLaunderingAppointees = historic
        };
    }
    private AntiMoneyLaundering GetAntiMoneyLaundering(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var antiMoneyLaunderingText = this.GetAntiMoneyLaunderingText(virksomhed);
        var antiMoneyLaunderingActivities = this.GetAntiMoneyLaunderingActivities(virksomhed);
        var isSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing = this.GetIsSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing(virksomhed);
        var antiMoneyLaunderingAppointees = this.GetAntiMoneyLaunderingAppointees(virksomhed);

        if (antiMoneyLaunderingText == null && !antiMoneyLaunderingActivities.Any() && !isSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing && antiMoneyLaunderingAppointees == null)
        {
            return null;
        }

        return new AntiMoneyLaundering
        {
            Text = antiMoneyLaunderingText,
            Activities = antiMoneyLaunderingActivities,
            IsSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing = isSubjectedToLawAboutMoneyLaunderingAndTerrorFinancing,
            AntiMoneyLaunderingAppointees = antiMoneyLaunderingAppointees
        };
    }
    private bool GetHasAudit(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return !virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.REVISION_FRAVALGT)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private RelationAuditors GetAuditor(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var auditors = this.GetRelations<Auditor>(virksomhedSummariskRelation, HovedTyper.REVISION);

        if (!auditors.Any())
        {
            return null;
        }

        var current = auditors
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationAuditor
            {
                RegistrationNumber = x.RegistrationNumber,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = auditors
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAuditor
            {
                RegistrationNumber = x.RegistrationNumber,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAuditors
        {
            Current = current,
            HistoricAuditors = historic
        };
    }
    private RelationAuditors GetAuditor(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var auditors = this.GetRelations<Auditor>(virksomhed, HovedTyper.REVISION)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!auditors.Any())
        {
            return null;
        }

        var current = auditors
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationAuditor
            {
                RegistrationNumber = x.RegistrationNumber,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = auditors
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAuditor
            {
                RegistrationNumber = x.RegistrationNumber,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAuditors
        {
            Current = current,
            HistoricAuditors = historic
        };
    }
    private Auditors GetAuditors(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var auditors = this.GetRelations<Auditor>(virksomhed, HovedTyper.REVISION);

        if (!auditors.Any())
        {
            return null;
        }

        var current = auditors
            .LastOrDefault(x => x.Period.IsValid());

        var historic = auditors
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new Auditors
        {
            Current = current,
            HistoricAuditors = historic
        };
    }
    private Auditors GetSustainabilityAuditors(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var sustainabilityAuditors = this.GetRelations<Auditor>(virksomhed, HovedTyper.BÆREDYGTIGHEDSREVISION);

        if (!sustainabilityAuditors.Any())
        {
            return null;
        }

        var current = sustainabilityAuditors
            .LastOrDefault(x => x.Period.IsValid());

        var historic = sustainabilityAuditors
            .Where(x => !x.Period.IsValid());

        return new Auditors
        {
            Current = current,
            HistoricAuditors = historic
        };
    }
    private bool GetIsOvertakenByFinansialStabilityAuthority(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.OVERTAGET_AF_FINANSIEL_STABILITET)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private Auditing GetAuditing(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var hasAudit = this.GetHasAudit(virksomhed);
        var auditors = this.GetAuditors(virksomhed);
        var sustainabilityAuditors = this.GetSustainabilityAuditors(virksomhed);
        var isOvertakenByFinansialStabilityAuthority = this.GetIsOvertakenByFinansialStabilityAuthority(virksomhed);

        if (!hasAudit && auditors == null && sustainabilityAuditors == null && !isOvertakenByFinansialStabilityAuthority)
        {
            return null;
        }

        return new Auditing
        {
            HasAudit = hasAudit,
            Auditor = auditors,
            SustainabilityAuditor = sustainabilityAuditors,
            IsOvertakenByFinancialStabilityAuthority = isOvertakenByFinansialStabilityAuthority
        };
    }
    private string GetSignatoryRule(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.TEGNINGSREGEL)
            .SelectMany(x => x.Vaerdier)
            .Select(x => x.Vaerdi)
            .LastOrDefault();
    }
    private RelationExecutives GetExecutive(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.DIREKTION,
            AttributVaerdier.DIREKTIONX,
            AttributVaerdier.DIREKTØR_I_IFS,
            AttributVaerdier.ADM_DIREKTØR,
            AttributVaerdier.DIREKTØR,
            AttributVaerdier.FILIALBESTYRERE,
            AttributVaerdier.FILIALBESTYRER,
            AttributVaerdier.FORVALTNINGSCHEF,
            AttributVaerdier.FORRETNINGSFØRER,
            AttributVaerdier.BESTYRELSE_DIREKTION
        };

        var executives = this.GetRelations<Executive>(virksomhedSummariskRelation, HovedTyper.LEDELSESORGAN, attributVærdier);

        if (!executives.Any())
        {
            return null;
        }

        var current = executives
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationExecutive
            {
                Title = x.Title,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .LastOrDefault();

        var historic = executives
            .Select(x => new RelationExecutive
            {
                Title = x.Title,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationExecutives
        {
            Current = current,
            HistoricExecutives = historic
        };
    }
    private RelationExecutives GetExecutive(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.DIREKTION,
            AttributVaerdier.DIREKTIONX,
            AttributVaerdier.DIREKTØR_I_IFS,
            AttributVaerdier.ADM_DIREKTØR,
            AttributVaerdier.DIREKTØR,
            AttributVaerdier.FILIALBESTYRERE,
            AttributVaerdier.FILIALBESTYRER,
            AttributVaerdier.FORVALTNINGSCHEF,
            AttributVaerdier.FORRETNINGSFØRER,
            AttributVaerdier.BESTYRELSE_DIREKTION
        };

        var executives = this.GetRelations<Executive>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!executives.Any())
        {
            return null;
        }

        var current = executives
            .Select(x => new RelationExecutive
            {
                Title = x.Title,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .LastOrDefault(x => x.Period.IsValid());

        var historic = executives
            .Select(x => new RelationExecutive
            {
                Title = x.Title,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationExecutives
        {
            Current = current,
            HistoricExecutives = historic
        };
    }
    private Executives GetExecutives(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.DIREKTION,
            AttributVaerdier.DIREKTIONX,
            AttributVaerdier.DIREKTØR_I_IFS,
            AttributVaerdier.ADM_DIREKTØR,
            AttributVaerdier.DIREKTØR,
            AttributVaerdier.FILIALBESTYRERE,
            AttributVaerdier.FILIALBESTYRER,
            AttributVaerdier.FORVALTNINGSCHEF,
            AttributVaerdier.FORRETNINGSFØRER,
            AttributVaerdier.BESTYRELSE_DIREKTION
        };

        var executives = this.GetRelations<Executive>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier);

        if (!executives.Any())
        {
            return null;
        }

        var current = executives
            .Where(x => x.Period.IsValid());

        var historic = executives
            .Where(x => !x.Period.IsValid());

        return new Executives
        {
            Current = current,
            HistoricExecutives = historic
        };
    }
    private RelationAuthorizedSignatories GetAuthorizedSignatories(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var authorizedSignatories = this.GetRelations<AuthorizedSignatory>(virksomhedSummariskRelation, HovedTyper.TEGNINGSBERETTIGEDE);

        if (!authorizedSignatories.Any())
        {
            return null;
        }

        var current = authorizedSignatories
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationAuthorizedSignatory
            {
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = authorizedSignatories
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAuthorizedSignatory
            {
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAuthorizedSignatories
        {
            Current = current,
            HistoricAuthorizedSignatories = historic
        };
    }
    private RelationAuthorizedSignatories GetAuthorizedSignatories(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var authorizedSignatories = this.GetRelations<AuthorizedSignatory>(virksomhed, HovedTyper.TEGNINGSBERETTIGEDE)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!authorizedSignatories.Any())
        {
            return null;
        }

        var current = authorizedSignatories
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationAuthorizedSignatory
            {
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = authorizedSignatories
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAuthorizedSignatory
            {
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAuthorizedSignatories
        {
            Current = current,
            HistoricAuthorizedSignatories = historic
        };
    }
    private AuthorizedSignatories GetAuthorizedSignatories(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var authorizedSignatories = this.GetRelations<AuthorizedSignatory>(virksomhed, HovedTyper.TEGNINGSBERETTIGEDE);

        if (!authorizedSignatories.Any())
        {
            return null;
        }

        var current = authorizedSignatories
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From);

        var historic = authorizedSignatories
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new AuthorizedSignatories
        {
            Current = current,
            HistoricAuthorizedSignatories = historic
        };
    }
    private RelationSpecialFinancialParticipants GetSpecialFinancialParticipants(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var specialFinancialParticipants = this.GetRelations<SpecialFinancialParticipant>(virksomhedSummariskRelation, HovedTyper.SÆRLIGE_FINANSIELLE_DELTAGERE);

        if (!specialFinancialParticipants.Any())
        {
            return null;
        }

        var current = specialFinancialParticipants
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationSpecialFinancialParticipant
            {
                Type = x.Type,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = specialFinancialParticipants
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationSpecialFinancialParticipant
            {
                Type = x.Type,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationSpecialFinancialParticipants
        {
            Current = current,
            HistoricSpecialFinancialParticipants = historic
        };
    }
    private RelationSpecialFinancialParticipants GetSpecialFinancialParticipants(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var specialFinancialParticipants = this.GetRelations<SpecialFinancialParticipant>(virksomhed, HovedTyper.SÆRLIGE_FINANSIELLE_DELTAGERE)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!specialFinancialParticipants.Any())
        {
            return null;
        }

        var current = specialFinancialParticipants
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationSpecialFinancialParticipant
            {
                Type = x.Type,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = specialFinancialParticipants
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationSpecialFinancialParticipant
            {
                Type = x.Type,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationSpecialFinancialParticipants
        {
            Current = current,
            HistoricSpecialFinancialParticipants = historic
        };
    }
    private SpecialFinancialParticipants GetSpecialFinancialParticipants(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var specialFinancialParticipants = this.GetRelations<SpecialFinancialParticipant>(virksomhed, HovedTyper.SÆRLIGE_FINANSIELLE_DELTAGERE);

        if (!specialFinancialParticipants.Any())
        {
            return null;
        }

        var current = specialFinancialParticipants
            .Where(x => x.Period.IsValid());

        var historic = specialFinancialParticipants
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new SpecialFinancialParticipants
        {
            Current = current,
            HistoricSpecialFinancialParticipants = historic
        };
    }
    private RelationLiableParticipants GetLiableParticipants(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var liableParticipants = this.GetRelations<LiableParticipant>(virksomhedSummariskRelation, HovedTyper.FULDT_ANSVARLIG_DELTAGERE);

        if (!liableParticipants.Any())
        {
            return null;
        }

        var current = liableParticipants
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationLiableParticipant
            {
                Role = x.Role,
                ElectionMethod = x.ElectionMethod,
                RegisteredCapital = x.RegisteredCapital,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = liableParticipants
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationLiableParticipant
            {
                Role = x.Role,
                ElectionMethod = x.ElectionMethod,
                RegisteredCapital = x.RegisteredCapital,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationLiableParticipants
        {
            Current = current,
            HistoricLiableParticipants = historic
        };
    }
    private RelationLiableParticipants GetLiableParticipants(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var liableParticipants = this.GetRelations<LiableParticipant>(virksomhed, HovedTyper.FULDT_ANSVARLIG_DELTAGERE)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!liableParticipants.Any())
        {
            return null;
        }
        
        var current = liableParticipants
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationLiableParticipant
            {
                Role = x.Role,
                ElectionMethod = x.ElectionMethod,
                RegisteredCapital = x.RegisteredCapital,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = liableParticipants
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationLiableParticipant
            {
                Role = x.Role,
                ElectionMethod = x.ElectionMethod,
                RegisteredCapital = x.RegisteredCapital,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationLiableParticipants
        {
            Current = current,
            HistoricLiableParticipants = historic
        };
    }
    private LiableParticipants GetLiableParticipants(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var liableParticipants = this.GetRelations<LiableParticipant>(virksomhed, HovedTyper.FULDT_ANSVARLIG_DELTAGERE);

        if (!liableParticipants.Any())
        {
            return null;
        }

        var current = liableParticipants
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From);

        var historic = liableParticipants
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new LiableParticipants
        {
            Current = current,
            HistoricLiableParticipants = historic
        };
    }
    private RelationLiquidator GetLiquidator(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.LIKVIDATOR,
            AttributVaerdier.LIKVIDATOR_IHT_VEDTÆGT,
            AttributVaerdier.LIKVIDATOR_E_VEDTÆGT,
            AttributVaerdier.LIKVDATOR
        };

        return this.GetRelations<Liquidator>(virksomhedSummariskRelation, HovedTyper.LEDELSESORGAN, attributVærdier)
            .Select(x => new RelationLiquidator
            {
                Title = x.Title,
                AppointedBy = x.AppointedBy,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To)
            .LastOrDefault();
    }
    private RelationLiquidator GetLiquidator(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.LIKVIDATOR,
            AttributVaerdier.LIKVIDATOR_IHT_VEDTÆGT,
            AttributVaerdier.LIKVIDATOR_E_VEDTÆGT,
            AttributVaerdier.LIKVDATOR
        };

        return this.GetRelations<Liquidator>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .Select(x => new RelationLiquidator
            {
                Title = x.Title,
                AppointedBy = x.AppointedBy,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To)
            .LastOrDefault();
    }
    private Liquidator[] GetLiquidators(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.LIKVIDATOR,
            AttributVaerdier.LIKVIDATOR_IHT_VEDTÆGT,
            AttributVaerdier.LIKVIDATOR_E_VEDTÆGT,
            AttributVaerdier.LIKVDATOR
        };

        return this.GetRelations<Liquidator>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier)
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To)
            .ToArray();
    }
    private Authority GetAuthority(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var signatoryRule = this.GetSignatoryRule(virksomhed);
        var executives = this.GetExecutives(virksomhed);
        var authorizedSignatories = this.GetAuthorizedSignatories(virksomhed);
        var specialFinancialParticipants = this.GetSpecialFinancialParticipants(virksomhed);
        var otherLiableParticipants = this.GetLiableParticipants(virksomhed);
        var liquidators = this.GetLiquidators(virksomhed);

        if (signatoryRule == null && executives == null && authorizedSignatories == null && specialFinancialParticipants == null && otherLiableParticipants == null && !liquidators.Any())
        {
            return null;
        }

        return new Authority
        {
            SignatoryRule = signatoryRule,
            Executives = executives,
            AuthorizedSignatories = authorizedSignatories,
            SpecialFinancialParticipants = specialFinancialParticipants,
            OtherLiableParticipants = otherLiableParticipants,
            Liquidators = liquidators
        };
    }
    private string GetOversightAuthority(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        const string DEFAULT_AUTHORITY = "Erhvervsstyrelsen";

        var authority = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.MYNDIGHED_ANDEN)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        return authority == null
            ? DEFAULT_AUTHORITY
            : $"{DEFAULT_AUTHORITY}, {authority.ToStringPretty()}";
    }
    private bool GetHasSocialEconomicOversight(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.SØV_IEF_TILSYN)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private bool GetHasSpecialLicensesOrConcessions(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var vaerdier = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.KONCESSIONSDATO)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .MaxBy(x => x.Periode.GyldigFra);

        return vaerdier != null;
    }
    private string GetOversightNotes(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var notes = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.TILSYN_KATEGORI)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi.ToStringPretty())
            .LastOrDefault();

        var socialEconomicNotes = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.SØV_IEF_TILSYN_TEKST)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        return notes == null || socialEconomicNotes == null
            ? notes ?? socialEconomicNotes
            : $"{notes}. {socialEconomicNotes}".Trim();
    }
    private RelationBoardMembers GetBoardMembers(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.HOVEDBESTYRELSE,
            AttributVaerdier.BESTYRELSE,
            AttributVaerdier.FORMAND,
            AttributVaerdier.BESTYRELSE_DIREKTION,
            AttributVaerdier.DIREKTIONSSEKRETÆR
        };

        var boardMembers = this.GetRelations<BoardMember>(virksomhedSummariskRelation, HovedTyper.LEDELSESORGAN, attributVærdier);

        if (!boardMembers.Any())
        {
            return null;
        }

        var current = boardMembers
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationBoardMember
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                AlternateFor = x.AlternateFor,
                IsDirective8Approved = x.IsDirective8Approved,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = boardMembers
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationBoardMember
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                AlternateFor = x.AlternateFor,
                IsDirective8Approved = x.IsDirective8Approved,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationBoardMembers
        {
            Current = current,
            HistoricBoardMembers = historic
        };
    }
    private RelationBoardMembers GetBoardMembers(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.HOVEDBESTYRELSE,
            AttributVaerdier.BESTYRELSE,
            AttributVaerdier.FORMAND,
            AttributVaerdier.BESTYRELSE_DIREKTION,
            AttributVaerdier.DIREKTIONSSEKRETÆR
        };

        var boardMembers = this.GetRelations<BoardMember>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!boardMembers.Any())
        {
            return null;
        }

        var current = boardMembers
            .Where(x => x.Period.IsValid())
            .Select(x => new RelationBoardMember
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                AlternateFor = x.AlternateFor,
                IsDirective8Approved = x.IsDirective8Approved,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = boardMembers
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationBoardMember
            {
                Title = x.Title,
                ElectionMethod = x.ElectionMethod,
                AlternateFor = x.AlternateFor,
                IsDirective8Approved = x.IsDirective8Approved,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationBoardMembers
        {
            Current = current,
            HistoricBoardMembers = historic
        };
    }
    private BoardMembers GetBoardMembers(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.HOVEDBESTYRELSE, 
            AttributVaerdier.BESTYRELSE, 
            AttributVaerdier.FORMAND,
            AttributVaerdier.BESTYRELSE_DIREKTION,
            AttributVaerdier.DIREKTIONSSEKRETÆR
        };

        var boardMembers = this.GetRelations<BoardMember>(virksomhed, HovedTyper.LEDELSESORGAN, attributVærdier);

        if (!boardMembers.Any())
        {
            return null;
        }

        var current = boardMembers
            .Where(x => x.Period.IsValid());

        var historic = boardMembers
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new BoardMembers
        {
            Current = current,
            HistoricBoardMembers = historic
        };
    }
    private RelationAssociationRepresentatives GetAssociationRepresentatives(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var associationRepresentative = this.GetRelations<AssociationRepresentative>(virksomhedSummariskRelation, HovedTyper.REPRÆSENTANTER);

        if (!associationRepresentative.Any())
        {
            return null;
        }

        var current = associationRepresentative
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationAssociationRepresentative
            {
                Title = x.Title,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = associationRepresentative
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAssociationRepresentative
            {
                Title = x.Title,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAssociationRepresentatives
        {
            Current = current,
            HistoricAssociationRepresentatives = historic
        };
    }
    private RelationAssociationRepresentatives GetAssociationRepresentatives(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var associationRepresentative = this.GetRelations<AssociationRepresentative>(virksomhed, HovedTyper.REPRÆSENTANTER)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!associationRepresentative.Any())
        {
            return null;
        }

        var current = associationRepresentative
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationAssociationRepresentative
            {
                Title = x.Title,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = associationRepresentative
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationAssociationRepresentative
            {
                Title = x.Title,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationAssociationRepresentatives
        {
            Current = current,
            HistoricAssociationRepresentatives = historic
        };
    }
    private AssociationRepresentatives GetAssociationRepresentatives(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var associationRepresentative = this.GetRelations<AssociationRepresentative>(virksomhed, HovedTyper.REPRÆSENTANTER);

        if (!associationRepresentative.Any())
        {
            return null;
        }

        var current = associationRepresentative
            .Where(x => x.Period.IsValid());
    
        var historic = associationRepresentative
            .Where(x => !x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new AssociationRepresentatives
        {
            Current = current,
            HistoricAssociationRepresentatives = historic
        };
    }
    private RelationFounder GetFounder(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        return this.GetRelations<Founder>(virksomhedSummariskRelation, HovedTyper.STIFTERE)
            .Select(x => new RelationFounder
            {
                RegistrationNumber = x.RegistrationNumber,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();
    }
    private RelationFounder GetFounder(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        if (externalId == null) 
            throw new ArgumentNullException(nameof(externalId));

        return this.GetRelations<Founder>(virksomhed, HovedTyper.STIFTERE)
            .Where(x => x.ExternalId == externalId)
            .Select(x => new RelationFounder
            {
                RegistrationNumber = x.RegistrationNumber,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();
    }
    private Founder[] GetFounders(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return this.GetRelations<Founder>(virksomhed, HovedTyper.STIFTERE);
    }
    private Governance GetGovernance(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var boardMembers = this.GetBoardMembers(virksomhed);
        var associationRepresentatives = this.GetAssociationRepresentatives(virksomhed);
        var oversight = this.GetGovernanceOversight(virksomhed);
        var founders = this.GetFounders(virksomhed);

        if (boardMembers == null && associationRepresentatives == null && oversight == null && founders == null)
        {
            return null;
        }

        return new Governance
        {
            Oversight = oversight,
            BoardMembers = boardMembers,
            AssociationRepresentatives = associationRepresentatives,
            Founders = founders
        };
    }
    private Oversight GetGovernanceOversight(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var oversightAuthority = this.GetOversightAuthority(virksomhed);
        var oversightNotes = this.GetOversightNotes(virksomhed);
        var hasSocialEconomicOversight = this.GetHasSocialEconomicOversight(virksomhed);
        var hasSpecialLicensesOrConcessions = this.GetHasSpecialLicensesOrConcessions(virksomhed);

        if (oversightAuthority == null && oversightNotes == null && !hasSocialEconomicOversight && !hasSpecialLicensesOrConcessions)
        {
            return null;
        }

        return new Oversight
        {
            Authority = oversightAuthority,
            HasSocialEconomicOversight = hasSocialEconomicOversight,
            HasSpecialLicensesOrConcessions = hasSpecialLicensesOrConcessions,
            Notes = oversightNotes
        };
    }
    private bool GetHasShareClasses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.KAPITALKLASSER)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private bool GetHasOnlyUnder5PercentOwnerships(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.EJERREGISTRERING_UNDER_5_PROCENT)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi != null && x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private bool GetHasPublicShareholderRegistry(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.OFFENTLIG_EJERBOG)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .Select(x => x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private RelationLegalOwner GetLegalOwner(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.EJERREGISTER,
            AttributVaerdier.EJERREGISTERX
        };

        return this.GetRelations<LegalOwner>(virksomhedSummariskRelation, HovedTyper.REGISTER, attributVærdier)
            .Select(x => new RelationLegalOwner
            {
                RegistrationNumber = x.RegistrationNumber,
                Equity = x.Equity,
                VotingRights = x.VotingRights,
                Notes = x.Notes,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();
    }
    private RelationLegalOwner GetLegalOwner(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.EJERREGISTER,
            AttributVaerdier.EJERREGISTERX
        };

        return this.GetRelations<LegalOwner>(virksomhed, HovedTyper.REGISTER, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .Select(x => new RelationLegalOwner
            {
                RegistrationNumber = x.RegistrationNumber,
                Equity = x.Equity,
                VotingRights = x.VotingRights,
                Notes = x.Notes,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();
    }
    private LegalOwner[] GetLegalOwners(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.EJERREGISTER, 
            AttributVaerdier.EJERREGISTERX
        };

        return this.GetRelations<LegalOwner>(virksomhed, HovedTyper.REGISTER, attributVærdier);
    }
    private RelationBeneficialOwner GetBeneficialOwner(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.REELLE_EJERE,
            AttributVaerdier.REELLE_EJERER,
            AttributVaerdier.REELLE_EJEREX
        };

        return this.GetRelations<BeneficialOwner>(virksomhedSummariskRelation, HovedTyper.REGISTER, attributVærdier)
            .Select(x => new RelationBeneficialOwner
            {
                Equity = x.Equity,
                OwnershipSpecial = x.OwnershipSpecial,
                VotingRights = x.VotingRights,
                CollateralVotingRights = x.CollateralVotingRights,
                Notes = x.Notes,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();
    }
    private RelationBeneficialOwner GetBeneficialOwner(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.REELLE_EJERE,
            AttributVaerdier.REELLE_EJERER,
            AttributVaerdier.REELLE_EJEREX
        };

        return this.GetRelations<BeneficialOwner>(virksomhed, HovedTyper.REGISTER, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .Select(x => new RelationBeneficialOwner
            {
                Equity = x.Equity,
                OwnershipSpecial = x.OwnershipSpecial,
                VotingRights = x.VotingRights,
                CollateralVotingRights = x.CollateralVotingRights,
                Notes = x.Notes,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();
    }
    private BeneficialOwner[] GetBeneficialOwners(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.REELLE_EJERE, 
            AttributVaerdier.REELLE_EJERER, 
            AttributVaerdier.REELLE_EJEREX
        };

        return this.GetRelations<BeneficialOwner>(virksomhed, HovedTyper.REGISTER, attributVærdier);
    }
    private Beneficiary GetBeneficiary(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var beneficiaryText = virksomhed.DeltagerRelation
            .SelectMany(x => x.Organisationer
                .Where(y => y.HovedType == HovedTyper.REGISTER)
                .SelectMany(y => y.Attributter)
                .Where(y => y.Type == AttributTyper.BEGUNSTIGET_GRUPPE)
                .SelectMany(z => z.Vaerdier
                    .Where(a => a.Periode.IsActive())
                    .Select(a => a.Vaerdi.ToStringPretty())))
            .LastOrDefault();

        var legalEntitlement = virksomhed.DeltagerRelation
            .SelectMany(x => x.Organisationer
                .Where(y => y.HovedType == HovedTyper.REGISTER)
                .SelectMany(y => y.Attributter)
                .Where(y => y.Type == AttributTyper.BEGUNSTIGET_RETSKRAV)
                .SelectMany(z => z.Vaerdier
                    .Where(a => a.Periode.IsActive())
                    .Select(a => a.Vaerdi.ToStringPretty())))
            .LastOrDefault();

        if (beneficiaryText == null && legalEntitlement == null)
        {
            return null;
        }

        return new Beneficiary
        {
            Text = beneficiaryText,
            LegalEntitlement = legalEntitlement
        };
    }
    private ParentCompany GetParentCompany(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.DeltagerRelation
            .Select(x =>
            {
                var organisation = x.Organisationer
                    .LastOrDefault(y => y.HovedType == HovedTyper.HOVEDSELSKAB);

                if (organisation?.EnhedsNummerOrganisation == null)
                {
                    return null;
                }

                var dateAndPeriod = organisation.MedlemsData
                    .SelectMany(y => y.Attributter
                        .Where(z => z.Type == AttributTyper.FUNKTION)
                        .SelectMany(z => z.Vaerdier))
                    .Where(y => y.Vaerdi == AttributVaerdier.HOVEDSELSKAB && y.Periode.IsActive())
                    .Select(vaerdier => new DateAndPeriod
                    {
                        Period =
                        {
                            From = vaerdier.Periode.GyldigFra
                        },
                        UpdatedAt = vaerdier.SidstOpdateret
                    })
                    .LastOrDefault();

                return new ParentCompany
                {
                    ExternalId = organisation.EnhedsNummerOrganisation,
                    Period = dateAndPeriod?.Period,
                    UpdatedAt = dateAndPeriod?.UpdatedAt
                };
            })
            .LastOrDefault(x => x != null);
    }
    private ParentCompany[] GetHistoricParentCompanies(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.DeltagerRelation
            .Select(x =>
            {
                var organisation = x.Organisationer
                    .LastOrDefault(y => y.HovedType == HovedTyper.HOVEDSELSKAB);

                if (organisation?.EnhedsNummerOrganisation == null)
                {
                    return null;
                }

                var dateAndPeriod = organisation.MedlemsData
                    .SelectMany(y => y.Attributter
                        .Where(z => z.Type == AttributTyper.FUNKTION)
                        .SelectMany(z => z.Vaerdier))
                    .Where(y => y.Vaerdi == AttributVaerdier.HOVEDSELSKAB && !y.Periode.IsActive())
                    .Select(vaerdier => new DateAndPeriod
                    {
                        Period =
                        {
                            From = vaerdier.Periode.GyldigFra,
                            To = vaerdier.Periode.GyldigTil
                        },
                        UpdatedAt = vaerdier.SidstOpdateret
                    })
                    .LastOrDefault();

                return new ParentCompany
                {
                    ExternalId = organisation.EnhedsNummerOrganisation,
                    Period = dateAndPeriod?.Period,
                    UpdatedAt = dateAndPeriod?.UpdatedAt
                };
            })
            .Where(x => x != null)
            .ToArray();
    }
    private MergersAndSplits[] GetMergers(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Fusioner
            .Select(x =>
            {
                var fusion = x.Indgaaende
                    .SelectMany(y => y.Vaerdier)
                    .FirstOrDefault(y => y.Vaerdi == AttributVaerdier.FUSION);

                if (fusion == null)
                {
                    return null;
                }

                return new MergersAndSplits
                {
                    ExternalId = x.EnhedsNummerOrganisation,
                    Period =
                    {
                        From = fusion.Periode.GyldigFra,
                        To = fusion.Periode.GyldigTil
                    },
                    UpdatedAt = fusion.SidstOpdateret
                };
            })
            .Where(x => x != null)
            .OrderBy(x => x.Period.From)
            .ToArray();
    }
    private MergersAndSplits[] GetSplits(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Spaltninger
            .Select(x =>
            {
                var spaltning = x.Indgaaende
                    .SelectMany(y => y.Vaerdier)
                    .FirstOrDefault(y => y.Vaerdi == AttributVaerdier.SPALTNING);

                if (spaltning == null)
                {
                    return null;
                }

                return new MergersAndSplits
                {
                    ExternalId = x.EnhedsNummerOrganisation,
                    Period =
                    {
                        From = spaltning.Periode.GyldigFra,
                        To = spaltning.Periode.GyldigTil
                    },
                    UpdatedAt = spaltning.SidstOpdateret
                };
            })
            .Where(x => x != null)
            .OrderBy(x => x.Period.From)
            .ToArray();
    }
    private Ownership GetOwnership(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var hasShareClasses = this.GetHasShareClasses(virksomhed);
        var hasOnlyUnder5PercentOwnerships = this.GetHasOnlyUnder5PercentOwnerships(virksomhed);
        var hasPublicShareholderRegistry = this.GetHasPublicShareholderRegistry(virksomhed);
        var beneficiary = this.GetBeneficiary(virksomhed);
        var parentCompanies = this.GetParentCompanies(virksomhed);
        var legalOwners = this.GetLegalOwners(virksomhed);
        var beneficialOwners = this.GetBeneficialOwners(virksomhed);
        var mergers = this.GetMergers(virksomhed);
        var splits = this.GetSplits(virksomhed);

        if (!hasShareClasses && !hasOnlyUnder5PercentOwnerships && !hasPublicShareholderRegistry && beneficiary == null && parentCompanies == null && !legalOwners.Any() && !beneficialOwners.Any() && !mergers.Any() && !splits.Any())
        {
            return null;
        }

        return new Ownership
        {
            HasShareClasses = hasShareClasses,
            HasOnlyUnder5PercentOwnerships = hasOnlyUnder5PercentOwnerships,
            HasPublicShareholderRegistry = hasPublicShareholderRegistry,
            Beneficiary = beneficiary,
            ParentCompany = parentCompanies,
            LegalOwners = legalOwners,
            BeneficialOwners = beneficialOwners,
            Mergers = mergers,
            DeMergers = splits
        };
    }
    private ParentCompanies GetParentCompanies(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var parentCompany = this.GetParentCompany(virksomhed);
        var historicParentCompanies = this.GetHistoricParentCompanies(virksomhed);

        if (parentCompany == null && !historicParentCompanies.Any())
        {
            return null;
        }

        return new ParentCompanies
        {
            Current = parentCompany,
            HistoricParentCompanies = historicParentCompanies
        };
    }
    private Bilaws GetBilaws(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.VEDTÆGT_SENESTE)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Bilaws
            {
                From = x.Periode.GyldigFra
            })
            .LastOrDefault();
    }
    private Bilaws[] GetHistoricBilaws(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.VEDTÆGT_SENESTE)
            .SelectMany(x => x.Vaerdier)
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new Bilaws
            {
                From = x.Periode.GyldigFra,
                To = x.Periode.GyldigTil
            })
            .ToArray();
    }
    private BilawsApproval GetBilawsApprovedAt(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var authority = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.STADFÆSTET_AF)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        var date = virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.STADFÆSTELSESDATO)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi.TryParseDateOnly())
            .LastOrDefault();

        if (authority == null && date == null)
        {
            return null;
        }

        return new BilawsApproval
        {
            Authority = authority,
            ApprovedAt = date
        };
    }
    private Bilawsses GetBilawses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var bilaws = this.GetBilaws(virksomhed);
        var historicBilaws = this.GetHistoricBilaws(virksomhed);
        var bilawsApproval = this.GetBilawsApprovedAt(virksomhed);

        if (bilaws == null && !historicBilaws.Any() && bilawsApproval == null)
        {
            return null;
        }

        return new Bilawsses
        {
            Current = bilaws,
            Approval = bilawsApproval,
            HistoricBilaws = historicBilaws
        };
    }
    private AuditorRegistration GetAuditorRegistration(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributes = virksomhed.DeltagerRelation
            .SelectMany(x => x.Organisationer
                .Where(y => y.HovedType == HovedTyper.REGISTER)
                .SelectMany(y => y.Attributter))
            .ToArray();

        var type = attributes
            .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_REVISIONSVIRKSOMHEDSTYPE)
            .SelectMany(z => z.Vaerdier
                .Where(a => a.Periode.IsActive())
                .Select(a => a.Vaerdi switch
                {
                    "PARAGRAF17_STK1" => "Paragraf 17, stk. 1",
                    _ => a.Vaerdi.ToStringPretty()
                }))
            .LastOrDefault();

        if (type == null)
        {
            return null;
        }

        var professionalNetwork = attributes
            .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_REVISIONSFAGLIGE_NETVAERK)
            .SelectMany(z => z.Vaerdier
                .Where(a => a.Periode.IsActive())
                .Select(a => a.Vaerdi.ToStringPretty()))
            .LastOrDefault();

        var publicInterest = attributes
            .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_PIE_SELVANGIVET)
            .SelectMany(z => z.Vaerdier
                .Where(a => a.Periode.IsActive())
                .Select(a => a.Vaerdi.ToStringPretty()))
            .LastOrDefault();

        var contactPerson = attributes
            .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_KONTAKTPERSON)
            .SelectMany(z => z.Vaerdier
                .Where(a => a.Periode.IsActive())
                .Select(a => a.Vaerdi))
            .LastOrDefault();

        var isHoldingCompany = attributes
            .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_HOLDINGVIRKSOMHED)
            .SelectMany(z => z.Vaerdier
                .Where(a => a.Periode.IsActive())
                .Select(a => a.Vaerdi.TryParseBool()))
            .LastOrDefault();

        var currentCertifiedAuditors = this.GetCertifiedAuditors(virksomhed);

        return new AuditorRegistration
        {
            Type = type,
            IsHoldingCompany = isHoldingCompany,
            PublicInterest = publicInterest,
            ContactPerson = contactPerson,
            ProfessionalNetwork = professionalNetwork,
            CertifiedAuditors = currentCertifiedAuditors
        };
    }
    private RelationCertifiedAuditors GetCertifiedAuditors(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var attributVærdier = new[]
        {
            AttributVaerdier.REVISIONSVIRKSOMHEDREGISTER,
            AttributVaerdier.REVISIONSVIRKSOMHEDSTEMMEBERETTIGEDE,
            AttributVaerdier.REVISIONSVIRKSOMHEDUDENLANDSKEMYNDIGHEDER
        };

        var certifiedAuditors = this.GetRelations<CertifiedAuditor>(virksomhedSummariskRelation, HovedTyper.REGISTER, attributVærdier);

        if (!certifiedAuditors.Any())
        {
            return null;
        }

        var current = certifiedAuditors
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationCertifiedAuditor
            {
                Role = x.Role,
                BusinessAddress = x.BusinessAddress,
                VotingRights = x.VotingRights,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = certifiedAuditors
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationCertifiedAuditor
            {
                Role = x.Role,
                BusinessAddress = x.BusinessAddress,
                VotingRights = x.VotingRights,
                EntityType = EntityType.Person,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationCertifiedAuditors
        {
            Current = current,
            HistoricCertifiedAuditors = historic
        };
    }
    private RelationCertifiedAuditors GetCertifiedAuditors(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.REVISIONSVIRKSOMHEDREGISTER,
            AttributVaerdier.REVISIONSVIRKSOMHEDSTEMMEBERETTIGEDE,
            AttributVaerdier.REVISIONSVIRKSOMHEDUDENLANDSKEMYNDIGHEDER
        };

        var certifiedAuditors = this.GetRelations<CertifiedAuditor>(virksomhed, HovedTyper.REGISTER, attributVærdier)
            .Where(x => x.ExternalId == externalId)
            .ToArray();

        if (!certifiedAuditors.Any())
        {
            return null;
        }

        var current = certifiedAuditors
            .Where(x => x.Period.IsValid())
            .OrderBy(x => x.Period.From)
            .Select(x => new RelationCertifiedAuditor
            {
                Role = x.Role,
                BusinessAddress = x.BusinessAddress,
                VotingRights = x.VotingRights,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .LastOrDefault();

        var historic = certifiedAuditors
            .Where(x => !x.Period.IsValid())
            .Select(x => new RelationCertifiedAuditor
            {
                Role = x.Role,
                BusinessAddress = x.BusinessAddress,
                VotingRights = x.VotingRights,
                EntityType = EntityType.Company,
                Period = x.Period,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To);

        return new RelationCertifiedAuditors
        {
            Current = current,
            HistoricCertifiedAuditors = historic
        };
    }
    private CertifiedAuditors GetCertifiedAuditors(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var attributVærdier = new[]
        {
            AttributVaerdier.REVISIONSVIRKSOMHEDREGISTER, 
            AttributVaerdier.REVISIONSVIRKSOMHEDSTEMMEBERETTIGEDE, 
            AttributVaerdier.REVISIONSVIRKSOMHEDUDENLANDSKEMYNDIGHEDER
        };

        var certifiedAuditors = this.GetRelations<CertifiedAuditor>(virksomhed, HovedTyper.REGISTER, attributVærdier);

        if (!certifiedAuditors.Any())
        {
            return null;
        }

        var current = certifiedAuditors
            .Where(x => x.Period.IsValid());

        var historic = certifiedAuditors
            .Where(x => !x.Period.IsValid());

        return new CertifiedAuditors
        {
            Current = current,
            HistoricCertifiedAuditors = historic
        };
    }
    private List<RelationType> GetActiveRoles(RelationRoles relationRoles)
    {
        if (relationRoles == null)
            throw new ArgumentNullException(nameof(relationRoles));

        var activeRoles = new List<RelationType>();

        if (relationRoles.Founder != null)
        {
            activeRoles
                .Add(RelationType.Founder);
        }

        if (relationRoles.LegalOwner != null && !relationRoles.LegalOwner.Period.To.HasValue && relationRoles.LegalOwner.Equity.Current != null)
        {
            activeRoles
                .Add(RelationType.LegalOwner);
        }

        if (relationRoles.BeneficialOwner != null && relationRoles.BeneficialOwner.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.BeneficialOwner);
        }

        if (relationRoles.Liquidator != null && relationRoles.Liquidator.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.Liquidator);
        }

        if (relationRoles.Auditors?.Current != null && relationRoles.Auditors.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.Auditor);
        }

        if (relationRoles.Executives?.Current != null && relationRoles.Executives.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.Executive);
        }

        if (relationRoles.BoardMembers?.Current != null && relationRoles.BoardMembers.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.BoardMember);
        }

        if (relationRoles.Managers?.Current != null && relationRoles.Managers.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.Manager);
        }

        if (relationRoles.AuthorizedSignatories?.Current != null && relationRoles.AuthorizedSignatories.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.AuthorizedSignatory);
        }

        if (relationRoles.AntiMoneyLaunderingAppointees?.Current != null && relationRoles.AntiMoneyLaunderingAppointees.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.AntiMoneyLaunderingAppointee);
        }

        if (relationRoles.SpecialFinancialParticipants?.Current != null && relationRoles.SpecialFinancialParticipants.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.SpecialFinancialParticipant);
        }

        if (relationRoles.LiableParticipants?.Current != null && relationRoles.LiableParticipants.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.LiableParticipant);
        }

        if (relationRoles.AssociationRepresentatives?.Current != null && relationRoles.AssociationRepresentatives.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.AssociationRepresentative);
        }

        if (relationRoles.CertifiedAuditors?.Current != null && relationRoles.CertifiedAuditors.Current.Period.IsValid())
        {
            activeRoles
                .Add(RelationType.CertifiedAuditor);
        }

        return activeRoles;
    }
    private List<RelationType> GetPersonActiveRoles(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var relationRoles = this.GetPersonRelationRoles(virksomhedSummariskRelation);

        return this.GetActiveRoles(relationRoles);
    }
    private RelationRoles GetPersonRelationRoles(VirksomhedSummariskRelation virksomhedSummariskRelation)
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        var founder = this.GetFounder(virksomhedSummariskRelation);
        var legalOwner = this.GetLegalOwner(virksomhedSummariskRelation);
        var beneficialOwner = this.GetBeneficialOwner(virksomhedSummariskRelation);
        var liquidator = this.GetLiquidator(virksomhedSummariskRelation);
        var auditors = this.GetAuditor(virksomhedSummariskRelation);
        var executives = this.GetExecutive(virksomhedSummariskRelation);
        var boardMembers = this.GetBoardMembers(virksomhedSummariskRelation);
        var managers = this.GetManagers(virksomhedSummariskRelation);
        var authorizedSignatories = this.GetAuthorizedSignatories(virksomhedSummariskRelation);
        var antiMoneyLaunderingAppointees = this.GetAntiMoneyLaunderingAppointees(virksomhedSummariskRelation);
        var specialFinancialParticipants = this.GetSpecialFinancialParticipants(virksomhedSummariskRelation);
        var liableParticipants = this.GetLiableParticipants(virksomhedSummariskRelation);
        var associationRepresentatives = this.GetAssociationRepresentatives(virksomhedSummariskRelation);
        var certifiedAuditors = this.GetCertifiedAuditors(virksomhedSummariskRelation);

        return new RelationRoles
        {
            Founder = founder,
            LegalOwner = legalOwner,
            BeneficialOwner = beneficialOwner,
            Liquidator = liquidator,
            Auditors = auditors,
            Executives = executives,
            BoardMembers = boardMembers,
            Managers = managers,
            AuthorizedSignatories = authorizedSignatories,
            AntiMoneyLaunderingAppointees = antiMoneyLaunderingAppointees,
            SpecialFinancialParticipants = specialFinancialParticipants,
            LiableParticipants = liableParticipants,
            AssociationRepresentatives = associationRepresentatives,
            CertifiedAuditors = certifiedAuditors
        };
    }
    private RelationRoles GetCompanyRelationRoles(VrVirksomhed virksomhed, string externalId)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var founder = this.GetFounder(virksomhed, externalId);
        var legalOwner = this.GetLegalOwner(virksomhed, externalId);
        var beneficialOwner = this.GetBeneficialOwner(virksomhed, externalId);
        var liquidator = this.GetLiquidator(virksomhed, externalId);
        var auditors = this.GetAuditor(virksomhed, externalId);
        var executives = this.GetExecutive(virksomhed, externalId);
        var boardMembers = this.GetBoardMembers(virksomhed, externalId);
        var managers = this.GetManagers(virksomhed, externalId);
        var authorizedSignatories = this.GetAuthorizedSignatories(virksomhed, externalId);
        var antiMoneyLaunderingAppointees = this.GetAntiMoneyLaunderingAppointees(virksomhed, externalId);
        var specialFinancialParticipants = this.GetSpecialFinancialParticipants(virksomhed, externalId);
        var liableParticipants = this.GetLiableParticipants(virksomhed, externalId);
        var associationRepresentatives = this.GetAssociationRepresentatives(virksomhed, externalId);
        var certifiedAuditors = this.GetCertifiedAuditors(virksomhed, externalId);

        return new RelationRoles
        {
            Founder = founder,
            LegalOwner = legalOwner,
            BeneficialOwner = beneficialOwner,
            Liquidator = liquidator,
            Auditors = auditors,
            Executives = executives,
            BoardMembers = boardMembers,
            Managers = managers,
            AuthorizedSignatories = authorizedSignatories,
            AntiMoneyLaunderingAppointees = antiMoneyLaunderingAppointees,
            SpecialFinancialParticipants = specialFinancialParticipants,
            LiableParticipants = liableParticipants,
            AssociationRepresentatives = associationRepresentatives,
            CertifiedAuditors = certifiedAuditors
        };
    }
    private ProductionUnitId[] GetActiveProductUnits(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.PEnheder
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new ProductionUnitId
            {
                ProductionUnitNumber = x.PNummer.ToString(),
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private ProductionUnitId[] GetHistoricProductUnits(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.PEnheder
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new ProductionUnitId
            {
                ProductionUnitNumber = x.PNummer.ToString(),
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private ProductionUnitCompany[] GetProductionUnitCompanies(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.VirksomhedsRelation
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new ProductionUnitCompany
            {
                RegistrationNumber = x.CvrNummer,
                Period =
                {
                    From = x.Periode.GyldigFra
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private ProductionUnitCompany[] GetHistoricProductionUnitCompanies(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.VirksomhedsRelation
            .Where(x => !x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => new ProductionUnitCompany
            {
                RegistrationNumber = x.CvrNummer,
                Period =
                {
                    From = x.Periode.GyldigFra,
                    To = x.Periode.GyldigTil
                },
                UpdatedAt = x.SidstOpdateret
            })
            .ToArray();
    }
    private ProductionUnits GetProductionUnitses(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        var activeProductUnits = this.GetActiveProductUnits(virksomhed);
        var historicProductUnits = this.GetHistoricProductUnits(virksomhed);

        if (!activeProductUnits.Any() && !historicProductUnits.Any())
        {
            return null;
        }

        return new ProductionUnits
        {
            Current = activeProductUnits,
            HistoricProductionUnits = historicProductUnits
        };
    }
    private ProductionUnitCompanies GetProductionUnitCompanieses(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        var currentCompanies = this.GetProductionUnitCompanies(produktionsEnhed);
        var historicCompanies = this.GetHistoricProductionUnitCompanies(produktionsEnhed);

        if (!currentCompanies.Any() && !historicCompanies.Any())
        {
            return null;
        }

        return new ProductionUnitCompanies
        {
            Current = currentCompanies,
            HistoricCompanies = historicCompanies
        };
    }
    private DateOnly? GetFoundedAt(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.LivsForloeb
            .Select(x => x.Periode.GyldigFra)
            .LastOrDefault();
    }
    private DateOnly? GetFoundedAt(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.LivsForloeb
            .Select(x => x.Periode.GyldigFra)
            .LastOrDefault();
    }
    private DateOnly? GetFoundedAt(VirksomhedSummarisk virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.LivsForloeb
            .Select(x => x.Periode.GyldigFra)
            .LastOrDefault();
    }
    private DateOnly? GetEffectiveStartedAt(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.VirksomhedMetadata.VirkningsDato;
    }
    private DateOnly? GetDissolvedAt(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.LivsForloeb
            .Select(x => x.Periode.GyldigTil)
            .LastOrDefault();
    }
    private DateOnly? GetDissolvedAt(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.LivsForloeb
            .Select(x => x.Periode.GyldigTil)
            .LastOrDefault();
    }
    private DateOnly? GetDissolvedAt(VirksomhedSummarisk virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.LivsForloeb
            .Select(x => x.Periode.GyldigTil)
            .LastOrDefault();
    }
    private DateOnly? GetCommercialFundApprovedAt(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.Attributter
            .Where(x => x.Type == AttributTyper.TILLADELSESDATO_FONDSMYNDIGHED)
            .SelectMany(x => x.Vaerdier)
            .Where(x => x.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .Select(x => x.Vaerdi.TryParseDateOnly())
            .LastOrDefault();
    }
    private bool GetIsProtectedFromAdvertisement(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.ReklameBeskyttet;
    }
    private bool GetIsProtectedFromAdvertisement(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.ReklameBeskyttet;
    }
    private EntityType GetEntityType(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.EnhedsType switch
        {
            EnhedsType.PERSON => EntityType.Person,
            EnhedsType.VIRKSOMHED => EntityType.Company,
            EnhedsType.PRODUKTIONSENHED => EntityType.ProductionUnit,
            EnhedsType.ANDEN_DELTAGER => EntityType.Other,
            _ => EntityType.Unknown
        };
    }
    private EntityType GetEntityType(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.EnhedsType switch
        {
            EnhedsType.PERSON => EntityType.Person,
            EnhedsType.VIRKSOMHED => EntityType.Company,
            EnhedsType.PRODUKTIONSENHED => EntityType.ProductionUnit,
            EnhedsType.ANDEN_DELTAGER => EntityType.Other,
            _ => EntityType.Unknown
        };
    }
    private EntityType GetEntityType(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.EnhedsType switch
        {
            EnhedsType.PERSON => EntityType.Person,
            EnhedsType.VIRKSOMHED => EntityType.Company,
            EnhedsType.PRODUKTIONSENHED => EntityType.ProductionUnit,
            EnhedsType.ANDEN_DELTAGER => EntityType.Other,
            _ => EntityType.Unknown
        };
    }
    private EntityType GetEntityType(VirksomhedSummarisk virksomhedSummarisk)
    {
        if (virksomhedSummarisk == null)
            throw new ArgumentNullException(nameof(virksomhedSummarisk));

        return virksomhedSummarisk.EnhedsType switch
        {
            EnhedsType.PERSON => EntityType.Person,
            EnhedsType.VIRKSOMHED => EntityType.Company,
            EnhedsType.PRODUKTIONSENHED => EntityType.ProductionUnit,
            EnhedsType.ANDEN_DELTAGER => EntityType.Other,
            _ => EntityType.Unknown
        };
    }
    private bool? GetIsHeaduarters(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.Hovedafdeling;
    }
    private bool? GetIsSupportingUnit(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.HjaelpeEnhed;
    }
    private bool? GetIsTemporary(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.Foreloebig;
    }
    private bool? GetHasConfidentiality(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.FortroligBeriget;
    }
    private Errors GetErrors(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return new Errors
        {
            HasImportErrors = virksomhed.FejlVedIndlaesning,
            HasRegistrationErrors = virksomhed.FejlRegistreret,
            Description = virksomhed.FejlBeskrivelse
        };
    }
    private Errors GetErrors(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return new Errors
        {
            HasImportErrors = produktionsEnhed.FejlVedIndlaesning,
            HasRegistrationErrors = produktionsEnhed.FejlRegistreret,
            Description = produktionsEnhed.FejlBeskrivelse
        };
    }
    private Errors GetErrors(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return new Errors
        {
            HasImportErrors = deltagerPerson.FejlVedIndlaesning,
            HasRegistrationErrors = deltagerPerson.FejlRegistreret,
            Description = deltagerPerson.FejlBeskrivelse
        };
    }
    private string GetExternalId(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.EnhedsNummer;
    }
    private string GetExternalId(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.EnhedsNummer;
    }
    private string GetExternalId(VirksomhedSummarisk deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.EnhedsNummer;
    }
    private DateTimeOffset? GetUpdatedAt(VrVirksomhed virksomhed)
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        return virksomhed.SidstOpdateret;
    }
    private DateTimeOffset? GetUpdatedAt(VrProduktionsEnhed produktionsEnhed)
    {
        if (produktionsEnhed == null)
            throw new ArgumentNullException(nameof(produktionsEnhed));

        return produktionsEnhed.SidstOpdateret;
    }
    private DateTimeOffset? GetUpdatedAt(VrDeltagerPerson deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.SidstOpdateret;
    }
    private DateTimeOffset? GetUpdatedAt(VirksomhedSummarisk deltagerPerson)
    {
        if (deltagerPerson == null)
            throw new ArgumentNullException(nameof(deltagerPerson));

        return deltagerPerson.SidstOpdateret;
    }
    private string GetKreditOplysningKodeText(long? kreditOplysningKode)
    {
        return kreditOplysningKode switch
        {
            1 => "Konkurs",
            3 => "Tvangsakkord",
            _ => null
        };
    }
    private string GetStatusKodeText(long? statusKode)
    {
        return statusKode switch
        {
            1 => "Dekret",
            2 => "Ophævelse af dekret",
            3 => "Regnskab og boafslutning",
            4 => "Andre meddelelser",
            5 => "Indkaldelse til fordringsprøvelse",
            6 => "Skiftesamling",
            7 => "Andre meddelelser",
            8 => "Åbning af forhandling",
            9 => "Stadfæstelse",
            _ => null
        };
    }
    private string GetBrancheAnsvarsKodeText(long? brancheAnsvarskode)
    {
        return brancheAnsvarskode switch
        {
            0 => null,
            15 => "Fremstilling / industri",
            65 => "Kreditmarked og forsikring",
            75 => "Offentlig enhed / ALPOS",
            76 => "Det offentlige område",
            96 => "Særligt DST kendskab",
            97 => "Henvendelse til ESR",
            99 => "CVR - bevis",
            _ => null
        };
    }
    private Address GetAddressOrDefault(Adresse adresse = null)
    {
        if (adresse == null)
        {
            return null;
        }

        var country = adresse.LandeKode;
        var cityName = adresse.PostDistrikt ?? adresse.ByNavn ?? adresse.Kommune.KommuneNavn;
        var cityCode = adresse.Kommune.KommuneKode.ToString();
        var zipCode = adresse.PostNummer;
        var streetName = adresse.VejNavn;
        var coStreetName = adresse.CoNavn;
        var postbox = adresse.Postboks;
        var houseNumberFrom = adresse.HusnummerFra;
        var houseNumberTo = adresse.HusnummerTil;
        var letterFrom = adresse.BogstavFra;
        var letterTo = adresse.BogstavTil;
        var floor = adresse.Etage;
        var door = adresse.SideDoer;
        var period = new Period
        {
            From = adresse.Periode.GyldigFra,
            To = adresse.Periode.GyldigTil
        };
        var updateAt = adresse.SidstOpdateret;

        return new Address
        {
            Country = country,
            City =
            {
                Name = cityName,
                Code = cityCode,
                PostalCode = zipCode
            },
            StreetName = streetName,
            CoStreetName = coStreetName,
            Postbox = postbox,
            HouseNumberFrom = houseNumberFrom,
            HouseNumberTo = houseNumberTo,
            LetterFrom = letterFrom,
            LetterTo = letterTo,
            Floor = floor,
            Door = door,
            Period = period,
            UpdatedAt = updateAt
        };
    }
    private T[] GetRelations<T>(VrVirksomhed virksomhed, string hovedType, params string[] attributVærdier) 
        where T : BaseRelation, new()
    {
        if (virksomhed == null)
            throw new ArgumentNullException(nameof(virksomhed));

        if (hovedType == null) 
            throw new ArgumentNullException(nameof(hovedType));

        return virksomhed.DeltagerRelation
            .Where(x => x.Organisationer
                .Any(y => y.HovedType == hovedType && y.Attributter
                    .Any(z => z.Type == AttributTyper.FUNKTION && z.Vaerdier
                        .Any(a => !attributVærdier.Any() || attributVærdier
                            .Any(b => b == a.Vaerdi?.ToUpper())))))
            .SelectMany(x =>
            {
                var organisation = x.Organisationer
                    .LastOrDefault(y => y.HovedType == hovedType && y.Attributter
                        .Any(z => z.Type == AttributTyper.FUNKTION && z.Vaerdier
                            .Any(q => !attributVærdier.Any() || attributVærdier
                                .Any(b => b == q.Vaerdi?.ToUpper()))));

                if (organisation == null || !organisation.MedlemsData.Any())
                {
                    return new List<T>();
                }

                var relationId = this.GetRelationId(x);
                var entityType = this.GetRelationEntityType(x);
                var name = this.GetRelationName(x);
                var address = this.GetRelationAddress(x);

                var relation = new T
                {
                    ExternalId = relationId,
                    EntityType = entityType,
                    Name = name,
                    Address = address
                };

                var registrationNumber = this.GetRelationRegistrationNumber(x);

                var relations = new List<T>();
                switch (relation)
                {
                    case Auditor:
                    case Manager:
                    case Executive:
                    case BoardMember:
                    case AuthorizedSignatory:
                    case AntiMoneyLaunderingAppointee:
                    case SpecialFinancialParticipant:
                    case LiableParticipant:
                    case Liquidator:
                    case AssociationRepresentative:
                    case CertifiedAuditor:
                    {
                        var dateAndPeriods = this.GetRelationDateAndPeriods(organisation);

                        foreach (var dateAndPeriod in dateAndPeriods)
                        {
                            relation.Period = dateAndPeriod.Period;
                            relation.UpdatedAt = dateAndPeriod.UpdatedAt;

                            relation = this.SetRelationProperties(relation, organisation, registrationNumber);

                            relations
                                .Add(relation);
                        }

                        break;
                    }

                    case Founder:
                    case LegalOwner:
                    case BeneficialOwner:
                    {
                        var dateAndPeriod = this.GetRelationDateAndPeriod(organisation, attributVærdier);

                        relation.Period = dateAndPeriod.Period;
                        relation.UpdatedAt = dateAndPeriod.UpdatedAt;

                        relation = this.SetRelationProperties(relation, organisation, registrationNumber);

                        relations
                            .Add(relation);

                        break;
                    }
                }

                return relations;
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To)
            .ToArray();
    }
    private T[] GetRelations<T>(VirksomhedSummariskRelation virksomhedSummariskRelation, string hovedType, params string[] attributVærdier)
        where T : BaseRelation, new()
    {
        if (virksomhedSummariskRelation == null)
            throw new ArgumentNullException(nameof(virksomhedSummariskRelation));

        if (hovedType == null)
            throw new ArgumentNullException(nameof(hovedType));

        return virksomhedSummariskRelation.Organisationer
            .Where(y => y.HovedType == hovedType && y.Attributter
                .Any(z => z.Type == AttributTyper.FUNKTION && z.Vaerdier
                    .Any(a => !attributVærdier.Any() || attributVærdier
                        .Any(b => b == a.Vaerdi?.ToUpper()))))
            .SelectMany(x =>
            {
                var relation = new T();
                var registrationNumber = this.GetRelationRegistrationNumber(virksomhedSummariskRelation.Virksomhed);

                var relations = new List<T>();
                switch (relation)
                {
                    case Auditor:
                    case Manager:
                    case Executive:
                    case BoardMember:
                    case AuthorizedSignatory:
                    case AntiMoneyLaunderingAppointee:
                    case SpecialFinancialParticipant:
                    case LiableParticipant:
                    case Liquidator:
                    case AssociationRepresentative:
                    case CertifiedAuditor:
                    {
                        var dateAndPeriods = this.GetRelationDateAndPeriods(x);

                        foreach (var dateAndPeriod in dateAndPeriods)
                        {
                            relation.Period = dateAndPeriod.Period;
                            relation.UpdatedAt = dateAndPeriod.UpdatedAt;

                            relation = this.SetRelationProperties(relation, x, registrationNumber);

                            relations
                                .Add(relation);
                        }

                        break;
                    }

                    case Founder:
                    case LegalOwner:
                    case BeneficialOwner:
                    {
                        var dateAndPeriod = this.GetRelationDateAndPeriod(x, attributVærdier);

                        relation.Period = dateAndPeriod.Period;
                        relation.UpdatedAt = dateAndPeriod.UpdatedAt;

                        relation = this.SetRelationProperties(relation, x, registrationNumber);

                        relations
                            .Add(relation);

                        break;
                    }
                }

                return relations;
            })
            .OrderBy(x => x.Period.From)
            .ThenBy(x => x.Period.To)
            .ToArray();
    }
    private T SetRelationProperties<T>(T relation, Organisation organisation, string registrationNumber)
        where T : BaseRelation, new()
    {
        if (relation == null)
            throw new ArgumentNullException(nameof(relation));

        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        switch (relation)
        {
            case Auditor auditor:
                auditor.RegistrationNumber = registrationNumber;

                return auditor as T;

            case Executive executive:
                executive.Title = this.GetRelationFunction(organisation, relation.Period);

                return executive as T;

            case Manager manager:
                manager.Title = this.GetRelationFunction(organisation, relation.Period);
                manager.ElectionMethod = this.GetRelationElectionMethod(organisation);

                return manager as T;

            case Liquidator liquidator:
                liquidator.Title = this.GetRelationFunction(organisation, relation.Period);
                liquidator.AppointedBy = this.GetRelationAppointedBy(organisation);

                return liquidator as T;

            case BoardMember boardMember:
                boardMember.Title = this.GetRelationFunction(organisation, relation.Period);
                boardMember.ElectionMethod = this.GetRelationElectionMethod(organisation);
                boardMember.AlternateFor = this.GetRelationAlternateFor(organisation);
                boardMember.IsDirective8Approved = this.GetRelationIsDirective8Approved(organisation);

                return boardMember as T;

            case AuthorizedSignatory authorizedSignatory:
                return authorizedSignatory as T;

            case AntiMoneyLaunderingAppointee antiMoneyLaunderingOfficer:
                antiMoneyLaunderingOfficer.Title = this.GetRelationFunction(organisation, relation.Period);

                return antiMoneyLaunderingOfficer as T;

            case LiableParticipant liableParticipant:
                liableParticipant.Role = this.GetRelationFunction(organisation, relation.Period);
                liableParticipant.RegisteredCapital = this.GetRelationRegisteredCapital(organisation);

                return liableParticipant as T;

            case SpecialFinancialParticipant specialFinancialParticipant:
                specialFinancialParticipant.Type = this.GetRelationFunction(organisation, relation.Period);

                return specialFinancialParticipant as T;

            case AssociationRepresentative associationRepresentative:
                associationRepresentative.Title = this.GetRelationFunction(organisation, relation.Period);

                return associationRepresentative as T;

            case Founder founder:
                founder.RegistrationNumber = registrationNumber;

                return founder as T;

            case LegalOwner legalOwner:
                legalOwner.RegistrationNumber = registrationNumber;
                legalOwner.Equity = this.GetRelationOwnershipPercentage(organisation);
                legalOwner.VotingRights = this.GetRelationVotingRightsPercentage(organisation);

                return legalOwner as T;

            case BeneficialOwner beneficialOwner:
                beneficialOwner.Equity = this.GetRelationOwnershipPercentage(organisation);
                beneficialOwner.VotingRights = this.GetRelationVotingRightsPercentage(organisation);
                beneficialOwner.CollateralVotingRights = this.GetRelationCollateralVotingRightsPercentage(organisation);
                beneficialOwner.OwnershipSpecial = this.GetRelationOwnershipSpecial(organisation);
                beneficialOwner.Notes = this.GetRelationOwnershipNotes(organisation);

                return beneficialOwner as T;

            case CertifiedAuditor certifiedAuditor:
                certifiedAuditor.Role = this.GetRelationFunction(organisation, relation.Period);
                certifiedAuditor.BusinessAddress = this.GetRelationBusinessAddress(organisation);
                certifiedAuditor.VotingRights = this.GetRelationCertifiedAuditorVotingRights(organisation);

                return certifiedAuditor as T;

            default:
                return relation;
        }
    }
    private EntityType GetRelationEntityType(DeltagerRelation deltagerRelation)
    {
        if (deltagerRelation == null)
            throw new ArgumentNullException(nameof(deltagerRelation));

        return deltagerRelation.Deltager.EnhedsType switch
        {
            EnhedsType.PERSON => EntityType.Person,
            EnhedsType.VIRKSOMHED => EntityType.Company,
            EnhedsType.PRODUKTIONSENHED => EntityType.ProductionUnit,
            EnhedsType.ANDEN_DELTAGER => EntityType.Other,
            _ => EntityType.Unknown
        };
    }
    private string GetRelationId(DeltagerRelation deltagerRelation)
    {
        if (deltagerRelation == null)
            throw new ArgumentNullException(nameof(deltagerRelation));

        return deltagerRelation.Deltager.EnhedsNummer;
    }
    private string GetRelationName(DeltagerRelation deltagerRelation)
    {
        if (deltagerRelation == null)
            throw new ArgumentNullException(nameof(deltagerRelation));

        return 
            deltagerRelation.Deltager.Navne
                .Where(x => x.Periode.IsActive())
                .Select(x => x.Navn)
                .LastOrDefault() ?? 
            deltagerRelation.Deltager.Navne
                .Select(x => x.Navn)
                .LastOrDefault();
    }
    private Address GetRelationAddress(DeltagerRelation deltagerRelation)
    {
        if (deltagerRelation == null)
            throw new ArgumentNullException(nameof(deltagerRelation));

        var address = deltagerRelation.Deltager.BeliggenhedsAdresse
            .Where(x => x.Periode.IsActive())
            .Select(this.GetAddressOrDefault)
            .LastOrDefault();

        if (address == null)
        {
            return null;
        }

        address.IsUnlisted = deltagerRelation.Deltager.AdresseHemmelig;
        address.IsAddressValidationDiscontinued = deltagerRelation.Deltager.AdresseOpdateringOphoert;

        return address;
    }
    private DateAndPeriod GetRelationDateAndPeriod(Organisation organisation, params string[] attributVærdier)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        if (attributVærdier == null) 
            throw new ArgumentNullException(nameof(attributVærdier));

        return this.GetRelationDateAndPeriods(organisation)
                   .LastOrDefault() ?? 
               organisation.Attributter
                   .SelectMany(z => z.Vaerdier)
                   .Where(x => !attributVærdier.Any() || attributVærdier
                       .Any(y => y == x.Vaerdi.ToUpper()))
                   .Select(z => new DateAndPeriod
                   {
                       Period =
                       {
                           From = z.Periode.GyldigFra,
                           To = z.Periode.GyldigTil
                       },
                       UpdatedAt = z.SidstOpdateret
                   })
                   .LastOrDefault();
    }
    private IEnumerable<DateAndPeriod> GetRelationDateAndPeriods(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        return organisation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(a => a.Type == AttributTyper.FUNKTION)
                .SelectMany(z => z.Vaerdier))
            .Select(z => new DateAndPeriod
            {
                Period =
                {
                    From = z.Periode.GyldigFra,
                    To = z.Periode.GyldigTil
                },
                UpdatedAt = z.SidstOpdateret
            })
            .DistinctBy(x => new
            {
                x.Period.From,
                x.Period.To
            });
    }
    private string GetRelationRegistrationNumber(DeltagerRelation deltagerRelation)
    {
        if (deltagerRelation == null)
            throw new ArgumentNullException(nameof(deltagerRelation));

        return deltagerRelation.Deltager.ForretningsNoegle;
    }
    private string GetRelationRegistrationNumber(VirksomhedSummarisk virksomhedSummarisk)
    {
        if (virksomhedSummarisk == null)
            throw new ArgumentNullException(nameof(virksomhedSummarisk));

        return virksomhedSummarisk.CvrNummer;
    }
    private string GetRelationFunction(Organisation organisation, Period period)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var function = organisation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(z => z.Type == AttributTyper.FUNKTION)
                .SelectMany(z => z.Vaerdier)
                .Where(z => z.Periode.GyldigFra == period.From && z.Periode.GyldigTil == period.To)
                .Select(z => z.Vaerdi switch
                {
                    "ADM. DIR." => "Adm. Direktør",
                    "RevisionsvirksomhedLedelse" => "Revisionsvirksomhed Ledelse",
                    "RevisionsvirksomhedLeder" => "Revisionsvirksomhed Leder",
                    "RevisionsvirksomhedStemmeberettiget" => "Revisionsvirksomhed Stemmeberettiget",
                    _ => z.Vaerdi?.ToStringPretty()
                }))
            .Aggregate(string.Empty, (current, x) => current + $"{x}, ")
            .Trim();

        return function.EndsWith(',')
            ? function[..^1]
            : function;
    }
    private string GetRelationAppointedBy(Organisation deltagerRelation)
    {
        if (deltagerRelation == null)
            throw new ArgumentNullException(nameof(deltagerRelation));

        return deltagerRelation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(z => z.Type == AttributTyper.UDNÆVNT_AF)
                .SelectMany(z => z.Vaerdier))
            .Select(z => z.Vaerdi?.ToStringPretty())
            .LastOrDefault();
    }
    private string GetRelationElectionMethod(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        return organisation.MedlemsData
            .SelectMany(q => q.Attributter
                .SelectMany(z => z.Vaerdier))
            .Where(z => z.Vaerdi == AttributTyper.VALGFORM)
            .Select(z => z.Vaerdi?.ToStringPretty())
            .LastOrDefault();
    }
    private string GetRelationAlternateFor(Organisation deltagerRelation)
    {
        if (deltagerRelation == null) 
            throw new ArgumentNullException(nameof(deltagerRelation));

        return deltagerRelation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(z => z.Type == AttributTyper.SUPPLEANT_FOR_DELTAGER_NR)
                .SelectMany(z => z.Vaerdier))
            .Select(z => z.Vaerdi?.ToStringPretty())
            .LastOrDefault();
    }
    private RegisteredCapital GetRelationRegisteredCapital(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var capital = organisation.MedlemsData
            .SelectMany(x => x.Attributter)
            .Where(x => x.Vaerdier.Any(y => y.Periode.IsActive()))
            .Where(x => x.Type == AttributTyper.KOMPLEMENTAR_INDSKUDSKAPITAL)
            .SelectMany(x => x.Vaerdier)
            .MaxBy(x => x.Periode.GyldigFra);

        if (capital == null)
        {
            return null;
        }

        var currency = organisation.MedlemsData
            .SelectMany(x => x.Attributter)
            .Where(x => x.Vaerdier.Any(y => y.Periode.IsActive()))
            .Where(x => x.Type == AttributTyper.KOMPLEMENTAR_INDSKUDSVALUTA)
            .SelectMany(x => x.Vaerdier)
            .Select(x => x.Vaerdi)
            .LastOrDefault();

        return new RegisteredCapital
        {
            Value = capital.Vaerdi?.TryParseDouble(DanishCvrService.numberFormatInfo),
            Currency = currency,
            Period =
            {
                From = capital.Periode.GyldigFra
            },
            UpdatedAt = capital.SidstOpdateret
        };
    }
    private bool? GetRelationIsDirective8Approved(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        return organisation.MedlemsData
            .SelectMany(x => x.Attributter
                .Where(y => y.Type == AttributTyper.REVISOR_REGISTRERING_GODKENDT_8DIREKTIV)
                .SelectMany(y => y.Vaerdier))
            .Where(x => x.Periode.IsActive())
            .Select(x => x.Vaerdi.TryParseBool())
            .LastOrDefault();
    }
    private Equity GetRelationOwnershipPercentage(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var ownership = organisation.MedlemsData
            .SelectMany(y => y.Attributter)
            .LastOrDefault(x => x.Type == AttributTyper.EJERANDEL_PROCENT);

        if (ownership == null)
        {
            return null;
        }

        var currentOwnership = ownership.Vaerdier
            .LastOrDefault(y => y.Periode.IsActive());

        var historicOwnerships = ownership.Vaerdier
            .Where(y => !y.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var datoOgPeriode = historicOwnerships
            .Where(x => x.Periode.GyldigTil.HasValue)
            .Select(x => new DatoOgPeriode
            {
                Periode = x.Periode,
                SidstOpdateret = x.SidstOpdateret
            })
            .LastOrDefault();

        Vaerdier shareClass = null;
        if (currentOwnership != null)
        {
            shareClass = organisation.MedlemsData
                .SelectMany(y => y.Attributter)
                .Where(x => x.Type == AttributTyper.EJERANDEL_KAPITALKLASSE)
                .SelectMany(x => x.Vaerdier)
                .LastOrDefault(x => x.Periode.IsActive() && x.Periode.GyldigFra == currentOwnership.Periode.GyldigFra && x.Periode.GyldigTil == currentOwnership.Periode.GyldigTil);
        }

        var historicShareClasses = historicOwnerships
            .Select(x => organisation.MedlemsData
                .SelectMany(y => y.Attributter)
                .Where(y => y.Type == AttributTyper.EJERANDEL_KAPITALKLASSE)
                .SelectMany(y => y.Vaerdier)
                .Where(y => y.Periode.IsActive() && y.Periode.GyldigFra == x.Periode.GyldigFra && y.Periode.GyldigTil == x.Periode.GyldigTil)
                .LastOrDefault(y => !y.Periode.IsActive()))
            .ToArray();

        return new Equity
        {
            Current = new Share
            {
                ShareClass = shareClass?.Vaerdi,
                SharePercentage = currentOwnership == null
                    ? 0.00D
                    : currentOwnership.Vaerdi.TryParseDouble(DanishCvrService.numberFormatInfo),
                Period =
                {
                    From = currentOwnership?.Periode.GyldigFra ?? datoOgPeriode?.Periode.GyldigTil?.AddDays(1)
                },
                UpdatedAt = currentOwnership?.SidstOpdateret ?? datoOgPeriode?.SidstOpdateret
            },
            HistoricValues = historicOwnerships
                .Select((y, i) => new Share
                {
                    ShareClass = historicShareClasses.ElementAtOrDefault(i)?.Vaerdi,
                    SharePercentage = double.Parse(y.Vaerdi, DanishCvrService.numberFormatInfo),
                    Period =
                    {
                        From = y.Periode.GyldigFra,
                        To = y.Periode.GyldigTil
                    },
                    UpdatedAt = y.SidstOpdateret
                })
        };
    }
    private VotingRights GetRelationVotingRightsPercentage(Organisation organisation)
    {
        if (organisation == null) 
            throw new ArgumentNullException(nameof(organisation));

        var votingRights = organisation.MedlemsData
            .SelectMany(y => y.Attributter)
            .LastOrDefault(x => x.Type == AttributTyper.EJERANDEL_STEMMERET_PROCENT);

        if (votingRights == null)
        {
            return null;
        }

        var currentVotingRights = votingRights.Vaerdier
            .LastOrDefault(y => y.Periode.IsActive());

        var historicVotingRights = votingRights.Vaerdier
            .Where(y => !y.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var datoOgPeriode = historicVotingRights
            .Where(x => x.Periode.GyldigTil.HasValue)
            .Select(x => new DatoOgPeriode
            {
                Periode = x.Periode,
                SidstOpdateret = x.SidstOpdateret
            })
            .LastOrDefault();

        return new VotingRights
        {
            Current = new Votes
            {
                Value = currentVotingRights == null
                    ? 0.00D
                    : currentVotingRights.Vaerdi.TryParseDouble(DanishCvrService.numberFormatInfo),
                Period =
                {
                    From = currentVotingRights?.Periode.GyldigFra ?? datoOgPeriode?.Periode.GyldigTil?.AddDays(1)
                },
                UpdatedAt = currentVotingRights?.SidstOpdateret ?? datoOgPeriode?.SidstOpdateret
            },
            HistoricValues = historicVotingRights
                .Select(y => new Votes
                {
                    Value = double.Parse(y.Vaerdi, DanishCvrService.numberFormatInfo),
                    Period =
                    {
                        From = y.Periode.GyldigFra,
                        To = y.Periode.GyldigTil
                    },
                    UpdatedAt = y.SidstOpdateret
                })
        };
    }
    private VotingRights GetRelationCollateralVotingRightsPercentage(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var votingRights = organisation.MedlemsData
            .SelectMany(y => y.Attributter)
            .LastOrDefault(x => x.Type == AttributTyper.EJERANDEL_SOM_PANT_STEMMERET_PROCENT);

        if (votingRights == null)
        {
            return null;
        }

        var currentVotingRights = votingRights.Vaerdier
            .LastOrDefault(y => y.Periode.IsActive());

        var historicVotingRights = votingRights.Vaerdier
            .Where(y => !y.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var datoOgPeriode = historicVotingRights
            .Where(x => x.Periode.GyldigTil.HasValue)
            .Select(x => new DatoOgPeriode
            {
                Periode = x.Periode,
                SidstOpdateret = x.SidstOpdateret
            })
            .LastOrDefault();

        return new VotingRights
        {
            Current = new Votes
            {
                Value = currentVotingRights == null
                    ? 0.00D
                    : currentVotingRights.Vaerdi.TryParseDouble(DanishCvrService.numberFormatInfo),
                Period =
                {
                    From = currentVotingRights?.Periode.GyldigFra ?? datoOgPeriode?.Periode.GyldigTil?.AddDays(1)
                },
                UpdatedAt = currentVotingRights?.SidstOpdateret ?? datoOgPeriode?.SidstOpdateret
            },
            HistoricValues = historicVotingRights
                .Select(y => new Votes
                {
                    Value = double.Parse(y.Vaerdi, DanishCvrService.numberFormatInfo),
                    Period =
                    {
                        From = y.Periode.GyldigFra,
                        To = y.Periode.GyldigTil
                    },
                    UpdatedAt = y.SidstOpdateret
                })
        };
    }
    private OwnershipSpecials GetRelationOwnershipSpecial(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var ownershipSpecial = organisation.MedlemsData
            .SelectMany(y => y.Attributter)
            .LastOrDefault(x => x.Type == AttributTyper.SÆRLIGE_EJERFORHOLD);

        if (ownershipSpecial == null)
        {
            return null;
        }

        var currentOwnershipSpecial = ownershipSpecial.Vaerdier
            .LastOrDefault(y => y.Periode.IsActive());

        var historicOwnershipSpecial = ownershipSpecial.Vaerdier
            .Where(y => !y.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var datoOgPeriode = historicOwnershipSpecial
            .Where(x => x.Periode.GyldigTil.HasValue)
            .Select(x => new DatoOgPeriode
            {
                Periode = x.Periode,
                SidstOpdateret = x.SidstOpdateret
            })
            .LastOrDefault();

        Vaerdier ownershipSpecialDescription = null;
        if (currentOwnershipSpecial != null)
        {
            ownershipSpecialDescription = organisation.MedlemsData
                .SelectMany(y => y.Attributter)
                .Where(x => x.Type == AttributTyper.SÆRLIGE_EJERFORHOLD_BESKRIVELSE)
                .SelectMany(x => x.Vaerdier)
                .LastOrDefault(x => x.Periode.IsActive() && x.Periode.GyldigFra == currentOwnershipSpecial.Periode.GyldigFra && x.Periode.GyldigTil == currentOwnershipSpecial.Periode.GyldigTil);
        }

        var historicOwnershipSpecialDescriptions = historicOwnershipSpecial
            .Select(x => organisation.MedlemsData
                .SelectMany(y => y.Attributter)
                .Where(y => y.Type == AttributTyper.SÆRLIGE_EJERFORHOLD_BESKRIVELSE)
                .SelectMany(y => y.Vaerdier)
                .Where(y => y.Periode.IsActive() && y.Periode.GyldigFra == x.Periode.GyldigFra && y.Periode.GyldigTil == x.Periode.GyldigTil)
                .LastOrDefault(y => !y.Periode.IsActive()))
            .ToArray();

        return new OwnershipSpecials
        {
            Current = new OwnershipSpecial 
            {
                Value = currentOwnershipSpecial?.Vaerdi,
                Notes = ownershipSpecialDescription?.Vaerdi,
                Period =
                {
                    From = currentOwnershipSpecial?.Periode.GyldigFra ?? datoOgPeriode?.Periode.GyldigTil?.AddDays(1)
                },
                UpdatedAt = currentOwnershipSpecial?.SidstOpdateret ?? datoOgPeriode?.SidstOpdateret
            },
            HistoricValues = historicOwnershipSpecial
                .Select((y, i) => new OwnershipSpecial
                {
                    Value = y.Vaerdi,
                    Notes = historicOwnershipSpecialDescriptions.ElementAtOrDefault(i)?.Vaerdi,
                    Period =
                    {
                        From = y.Periode.GyldigFra,
                        To = y.Periode.GyldigTil
                    },
                    UpdatedAt = y.SidstOpdateret
                })
        };
    }
    private string GetRelationOwnershipNotes(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var notes = organisation.MedlemsData
            .SelectMany(x => x.Attributter)
            .Where(x => x.Vaerdier.Any(y => y.Periode.IsActive()))
            .Where(x => x.Type == AttributTyper.BETYDELIG_INDFLYDELSE_VIA_ROLLE)
            .SelectMany(x => x.Vaerdier)
            .OrderBy(x => x.Periode.GyldigFra)
            .Aggregate("", (current, x) => $"{current}{x.Vaerdi}. ")
            .Trim();

        if (string.IsNullOrEmpty(notes))
        {
            return null;
        }

        return notes;
    }
    private string GetRelationBusinessAddress(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        return organisation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(z => z.Type == AttributTyper.FORRETNINGSADRESSE)
                .SelectMany(z => z.Vaerdier))
            .Select(z => z.Vaerdi?.ToStringPretty())
            .LastOrDefault();
    }
    private AuditorVotingRights GetRelationCertifiedAuditorVotingRights(Organisation organisation)
    {
        if (organisation == null)
            throw new ArgumentNullException(nameof(organisation));

        var votingRights = organisation.MedlemsData
            .SelectMany(y => y.Attributter)
            .LastOrDefault(x => x.Type == AttributTyper.REVISOR_REGISTRERING_STEMMEANDEL_PROCENT);

        var currentVotingRights = votingRights?.Vaerdier
            .LastOrDefault(y => y.Periode.IsActive());

        var historicVotingRights = votingRights?.Vaerdier
            .Where(y => !y.Periode.IsActive())
            .OrderBy(x => x.Periode.GyldigFra)
            .ToArray();

        var datoOgPeriode = historicVotingRights?
            .Where(x => x.Periode.GyldigTil.HasValue)
            .Select(x => new DatoOgPeriode
            {
                Periode = x.Periode,
                SidstOpdateret = x.SidstOpdateret
            })
            .LastOrDefault();

        var type = organisation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(z => z.Type == AttributTyper.REVISOR_REGISTRERING_STEMMEANDEL_INDEHAVERTYPE)
                .SelectMany(z => z.Vaerdier))
            .Select(z => z.Vaerdi?.ToStringPretty())
            .LastOrDefault();

        var exception = organisation.MedlemsData
            .SelectMany(q => q.Attributter
                .Where(z => z.Type == AttributTyper.REVISOR_REGISTRERING_STEMMEANDEL_INDEHAVERUNDTAGELSE)
                .SelectMany(z => z.Vaerdier))
            .Select(z => z.Vaerdi?.ToStringPretty())
            .LastOrDefault();

        return new AuditorVotingRights
        {
            Type = type,
            Exception = exception,
            Current = new Votes
            {
                Value = currentVotingRights?.Vaerdi?.TryParseDouble(DanishCvrService.numberFormatInfo) ?? 0.00D,
                Period =
                {
                    From = currentVotingRights?.Periode.GyldigFra ?? datoOgPeriode?.Periode.GyldigTil?.AddDays(1)
                },
                UpdatedAt = currentVotingRights?.SidstOpdateret ?? datoOgPeriode?.SidstOpdateret
            },
            HistoricValues = historicVotingRights?
                .Select(y => new Votes
                {
                    Value = double.Parse(y.Vaerdi, DanishCvrService.numberFormatInfo),
                    Period =
                    {
                        From = y.Periode.GyldigFra,
                        To = y.Periode.GyldigTil
                    },
                    UpdatedAt = y.SidstOpdateret
                }) ?? new List<Votes>()
        };
    }

    private string GetDebugRawJson(object obj)
    {
        if (obj == null) 
            throw new ArgumentNullException(nameof(obj));
        
        if (Debugger.IsAttached)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, DanishCvrService.jsonSerializerSettings);
        }

        if (this.options.IncludeRawJson)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, DanishCvrService.jsonSerializerSettings);
        }

        return null;
    }
}