# DanishCvr
[![Build and Deploy](https://github.com/Vivet/DanishCvr/actions/workflows/build-and-deploy.yml/badge.svg)](https://github.com/Vivet/DanishCvr/actions/workflows/build-and-deploy.yml)
[![NuGet](https://img.shields.io/nuget/dt/DanishCvr.svg)](https://www.nuget.org/packages/DanishCvr/)
[![NuGet](https://img.shields.io/nuget/v/DanishCvr.svg)](https://www.nuget.org/packages/DanishCvr/)

A .NET library for looking up Danish companies, production units, persons and ownership relations in the Central Business Register (CVR).

It wraps the CVR system-to-system service from Erhvervsstyrelsen (Elasticsearch) in a typed, async API, so you do not have to write Elasticsearch queries or parse the raw documents yourself. All data is mapped from CVR into strongly typed response models, with current values and the history of earlier ones side by side.

What you can do:

* Look up companies by CVR number, external id or number prefix.
* Search and auto-complete companies and persons, with criteria, sorting and paging.
* Get production units, registration texts and relations.
* Get the reference lists for business types, statuses and industries (DB07).
* Walk the whole register with the batch service, for building a local copy.

A company response covers, among other things:

* Names and alternative names, addresses, contact details, business type, industry and status (including credit status).
* Founders, founding date and financial years, employment, equity and financial purpose.
* Auditors and certified auditors, including their registration.
* Legal owners, beneficial owners, beneficiaries, board members, executives, managers, liquidators and authorized signatories.
* Liable participants, parent companies, mergers and splits, bylaws, anti-money-laundering appointees and association representatives.

Production units, persons and the relations between them (who is owner, board member, auditor and so on) are mapped the same way.

***

### Configuration and Registration

Add the following to the configuration.

```json
"DanishCvr": {
    "Username": "",
    "Password": ""
}
```

💡 You need a CVR user to fill in the credentials. See [Get Access to CVR Data](#get-access-to-cvr-data).

Then register the services in the service collection, in start-up.

```csharp
services
    .AddDanishCvr();
```

***

### Get Access to CVR Data

The CVR data is free, but you need a user from Erhvervsstyrelsen. The library authenticates with the user id and password you receive.

1. Open the article [System-til-systemadgang til CVR-data](https://datacvr.virk.dk/artikel/system-til-system-adgang-til-cvr-data) on datacvr.virk.dk.
2. Follow the link "Anmod om adgang til system-til-systemløsningen" and fill in [the form](https://blanket.virk.dk/blanketafvikler/orbeon/fr/public_v/6_7665510f1e2bc7bd3b9c6acd0d32b4a773f13517/new).
3. Wait for the user to be created. Erhvervsstyrelsen states a normal processing time of three weeks. You then receive your user details by email.
4. Put the user id and password in the configuration shown under [Configuration and Registration](#configuration-and-registration).

More information:

* [Kom godt i gang med Elasticsearch](https://erhvervsstyrelsen.dk/kom-godt-igang-med-elasticSearch), the official guide to the search service (Elasticsearch 6.8).
* Questions about CVR data: cvrselvbetjening@erst.dk. Erhvervsstyrelsen does not give support on Elasticsearch itself.

***

### Services and Methods

Both services are registered by `AddDanishCvr()`.

#### IDanishCvrService

| Method | Description | Returns |
|---|---|---|
| `GetCompanyBusinessTypesAsync` | All business types (company forms) with code, abbreviation and name. | `BusinessTypesResponse` |
| `GetCompanyStatusesAsync` | All company statuses. | `StatusesResponse` |
| `GetCompanyIndustriesAsync` | All industry codes (DB07). | `IndustriesResponse` |
| `GetCompanyAsync` | One company by CVR number. | `CompanyResponse` |
| `GetCompanyByExternalIdAsync` | One company by external id. | `CompanyResponse` |
| `GetCompaniesAsync` (numbers) | Several companies by a list of CVR numbers. | `CompaniesSearchResponse` |
| `GetCompaniesAsync` (prefix) | Companies whose CVR number starts with a prefix, with paging. | `CompaniesResponse` |
| `AutoCompleteCompaniesAsync` | Company suggestions for a partial name. | `CompaniesAutoCompleteResponse` |
| `SearchCompaniesAsync` | Search companies by criteria, with sorting and paging. | `CompaniesSearchResponse` |
| `GetCompanyRegistrationTextsAsync` | Registration texts for a company, with paging. | `RegistrationTextsResponse` |
| `GetProductionUnitAsync` | One production unit by id. | `ProductionUnitResponse` |
| `GetProductionUnitsAsync` | The production units of a company, optionally including historic ones. | `ProductionUnitsResponse` |
| `SearchProductionUnitsAsync` | Search production units by criteria. | `ProductionUnitsSearchResponse` |
| `GetPersonAsync` | One person by external id. | `PersonResponse` |
| `AutoCompletePersonsAsync` | Person suggestions for a partial name. | `PersonsAutoCompleteResponse` |
| `SearchPersonsAsync` | Search persons by criteria. | `PersonsSearchResponse` |
| `GetRelationsAsync` | Relations of a company or person (owners, board and more), optionally including historic ones. | `RelationsResponse` |

#### IDanishCvrBatchService

| Method | Description | Returns |
|---|---|---|
| `SearchAllAsync` | Walks all CVR number prefixes in parallel and hands each batch of companies to a callback. Used to build a local copy of the register. | `DebugInformation` |
