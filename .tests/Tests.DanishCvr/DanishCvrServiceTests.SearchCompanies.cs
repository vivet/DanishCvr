using System;
using System.Linq;
using System.Threading.Tasks;
using DanishCvr.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Range = DanishCvr.Types.Range;
using DanishCvr.Types;
using DanishCvr.Requests.Models.Enums;
using DanishCvr.Requests;
using DanishCvr.Requests.Models;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task SearchCompaniesByAllTest()
    {
        const string NAME = "11x Mod";
        string[] businessTypeCodes = ["80", "90", "260"];
        string[] industryCodes = ["620100", "642020"];
        const int MIN = 0;
        const int MAX = 200;
        var foundedAtFrom = new DateOnly(2018, 1, 1);
        var foundedAtTo = new DateOnly(2020, 12, 31);
        var dissolvedAtFrom = new DateOnly(2021, 1, 1);
        var dissolvedAtTo = new DateOnly(2024, 12, 31);

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    },
                    BusinessTypeCodes = businessTypeCodes,
                    IndustryCodes = industryCodes,
                    NumberOfEmployees = new Range
                    {
                        From = MIN,
                        To = MAX
                    },
                    FoundedAt = new Period
                    {
                        From = foundedAtFrom,
                        To = foundedAtTo
                    },
                    DissolvedAt = new Period
                    {
                        From = dissolvedAtFrom,
                        To = dissolvedAtTo
                    },
                    IsActive = false,
                    IsProtectedFromAdvertisment = true
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Length);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameTest()
    {
        const string NAME = "Vivet";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME,
                        IncludeAlternativeNames = false,
                        IncludeHistoricNames = false
                    }
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(18, results.Length);

        var all = results.All(x => x.Company.Name.Value.ToUpper().Contains(NAME.ToUpper()));
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameWhenAlternativeNameTest()
    {
        const string ALTERNATIVE_NAME = "Biz ap";
        const string EXPECTED_NAME = "Vivet Software ApS";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = ALTERNATIVE_NAME,
                        IncludeAlternativeNames = true,
                        IncludeHistoricNames = false
                    }
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

        var first = results.FirstOrDefault();
        Assert.IsNotNull(first);
        Assert.AreEqual(EXPECTED_NAME, first.Company.Name.Value);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameWhenAlternativeNameAndNotIncludedTest()
    {
        const string ALTERNATIVE_NAME = "Biz ap";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = ALTERNATIVE_NAME,
                        IncludeAlternativeNames = false,
                        IncludeHistoricNames = false
                    }
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Length);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameWhenHistoricNameTest()
    {
        const string HISTORIC_NAME = "Darth Vivet";
        const string EXPECTED_NAME = "Vivet Software ApS";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = HISTORIC_NAME,
                        IncludeAlternativeNames = false,
                        IncludeHistoricNames = true
                    }
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

        var first = results.FirstOrDefault();
        Assert.IsNotNull(first);
        Assert.AreEqual(EXPECTED_NAME, first.Company.Name.Value);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameWhenHistoricNameAndNotIncludedTest()
    {
        const string HISTORIC_NAME = "Darth Vivet";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = HISTORIC_NAME,
                        IncludeAlternativeNames = false,
                        IncludeHistoricNames = false
                    }
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Length);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameAndWhitespaceTest()
    {
        const string NAME = "11x Mod";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    }
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Length);

        var all = results.All(x => x.Company.Name.Value.ToUpper().Contains(NAME.ToUpper()));
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameAndIncompletePhraseTest()
    {
        const string NAME = "micros";
        const string EXPECTED_NAME = "MICROSTAR HOLDING ApS";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    }
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

        var first = results.FirstOrDefault();
        Assert.IsNotNull(first);
        Assert.AreEqual(EXPECTED_NAME, first.Company.Name.Value);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameAndIncompletePhraseAndMultipleWordsTest()
    {
        const string NAME_PART_ONE = "Vivet";
        const string NAME_PART_TWO = "Soft";
        const string NAME = $"{NAME_PART_ONE} {NAME_PART_TWO}";
        const string EXPECTED_NAME = "Vivet Software ApS";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    }
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

        var first = results.FirstOrDefault();
        Assert.IsNotNull(first);
        Assert.AreEqual(EXPECTED_NAME, first.Company.Name.Value);
    }

    [TestMethod]
    public async Task SearchCompaniesByNameAndSortByNameAscendingTest()
    {
        const string NAME = "Microsoft";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    }
                },
                Paging =
                {
                    Count = 250
                },
                Sorting =
                {
                    By = CompanySortBy.Name
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(15, results.Length);

        var result = results
            .Select(x => x.Company.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) <= 0, $"The list is not in correct order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task SearchCompaniesByNameAndSortByNameDescendingTest()
    {
        const string NAME = "Microsoft";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    }
                },
                Paging =
                {
                    Count = 250
                },
                Sorting =
                {
                    By = CompanySortBy.Name,
                    Direction = SortDirection.Descending
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(15, results.Length);

        var result = results
            .Select(x => x.Company.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) >= 0, $"The list is not in correct descending order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task SearchCompaniesByBusinessTypeCodesTest()
    {
        string[] businessTypeCodes = ["80", "90", "260"];

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    BusinessTypeCodes = businessTypeCodes
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.Type.Code is "80" or "90" or "260");
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByWithinTest()
    {
        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Within = new Within
                    {
                        Location = new Location
                        {
                            Latitude = 55.14456823868534,
                            Longitude = 8.494448565055759
                        },
                        Radius = 1
                    }
                },
                Paging =
                {
                    Count = 500
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.IsTrue(results.Length > 400);
        Assert.IsTrue(results.Length < 500);
    }

    [TestMethod]
    public async Task SearchCompaniesByIndustryCodesTest()
    {
        string[] industryCodes = ["620100", "642020"];

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    IndustryCodes = industryCodes
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.Industry.Code is "620100" or "642020");
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByIndustryCodesWhenSecondaryIndustryTest()
    {
        const string EXPECTED_NAME = "MICROSOFT DANMARK ApS";

        string[] industryCodes = ["461890"];

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = "Microsoft Danmark"
                    },
                    IndustryCodes = industryCodes
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Length);

        var first = results.FirstOrDefault();
        Assert.IsNotNull(first);
        Assert.AreEqual(EXPECTED_NAME, first.Company.Name.Value);
    }

    [TestMethod]
    public async Task SearchCompaniesByStatusTest()
    {
        const string STATUS = "Normal";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Statuses =
                    [
                        STATUS
                    ]
                },
                Paging =
                {
                    Count = 250,
                    Skip = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.Status.Value == STATUS);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByStatusWhenWhitespaceTest()
    {
        const string STATUS = "OPLØST EFTER ERKLÆRING";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Statuses =
                    [
                        STATUS
                    ]
                },
                Paging =
                {
                    Count = 250,
                    Skip = 0
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.Status.Value == STATUS.ToStringPretty());
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByStatusWhenAktivTest()
    {
        const string STATUS = "Aktiv";
        const string EXPECTED_STATUS = "Normal";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Statuses =
                    [
                        STATUS
                    ]
                },
                Paging =
                {
                    Count = 250,
                    Skip = 500
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.Status.Value == EXPECTED_STATUS);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByStatusAndMultipleTest()
    {
        const string STATUS1 = "Opløst efter grænseoverskridende fusion";
        const string STATUS2 = "Opløst efter grænseoverskridende hjemstedsflytning";

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    Statuses =
                    [
                        STATUS1,
                        STATUS2
                    ]
                },
                Paging =
                {
                    Count = 500,
                    Skip = 0
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);

        var all = results.All(x => x.Company.Status.Value is STATUS1 or STATUS2);
        Assert.IsTrue(all);

        var existsStatus1 = results.Any(x => x.Company.Status.Value == STATUS1);
        Assert.IsTrue(existsStatus1);

        var existsStatus2 = results.Any(x => x.Company.Status.Value == STATUS2);
        Assert.IsTrue(existsStatus2);
    }

    [TestMethod]
    public async Task SearchCompaniesByNumberOfEmployeesTest()
    {
        const int MIN = 10;
        const int MAX = 20;

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    NumberOfEmployees = new Range
                    {
                        From = MIN,
                        To = MAX
                    }
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.Employees == null || x.Company.Employees.NumberOfEmployees is >= MIN and <= MAX);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByNumberOfEmployeesAndSortingByNumberOfEmployeesAscendingTest()
    {
        const int MIN = 10;
        const int MAX = 20;

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    NumberOfEmployees = new Range
                    {
                        From = MIN,
                        To = MAX
                    }
                },
                Paging =
                {
                    Count = 250
                },
                Sorting =
                {
                    By = CompanySortBy.NumberOfEmployees
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var result = results
            .Select(x => x.Company.Employees.NumberOfEmployees)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            Assert.IsTrue(result[i - 1] <= result[i], $"The array is not in correct ascending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        }
    }

    [TestMethod]
    public async Task SearchCompaniesByNumberOfEmployeesAndSortingByNumberOfEmployeesDescendingTest()
    {
        const int MIN = 10;
        const int MAX = 20;

        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    NumberOfEmployees = new Range
                    {
                        From = MIN,
                        To = MAX
                    }
                },
                Paging =
                {
                    Count = 250
                },
                Sorting =
                {
                    By = CompanySortBy.NumberOfEmployees
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var result = results
            .Select(x => x.Company.Employees.NumberOfEmployees)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            Assert.IsTrue(result[i - 1] >= result[i], $"The array is not in correct descending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        }
    }

    [TestMethod]
    public async Task SearchCompaniesByFoundedAtTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //var from = new DateOnly(2010, 1, 1);
        //var to = new DateOnly(2020, 12, 31);

        //var response = await this.DanishCvrService
        //    .SearchCompaniesAsync(new SearchCompaniesRequest
        //    {
        //        Criteria =
        //        {
        //            FoundedAt = new Period
        //            {
        //                From = from,
        //                To = to
        //            }
        //        },
        //        Paging =
        //        {
        //            Count = 250
        //        }
        //    });

        //var results = response.Results.ToArray();

        //Assert.IsNotNull(results);
        //Assert.AreEqual(250, results.Length);

        //var all = results.All(x => x.Company.FoundedAt >= from && x.Company.FoundedAt <= to);
        //Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByFoundedAtAndSortingByFoundedAtAscendingTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //var from = new DateOnly(2010, 1, 1);
        //var to = new DateOnly(2020, 12, 31);

        //var response = await this.DanishCvrService
        //    .SearchCompaniesAsync(new SearchCompaniesRequest
        //    {
        //        Criteria =
        //        {
        //            FoundedAt = new Period
        //            {
        //                From = from,
        //                To = to
        //            }
        //        },
        //        Paging =
        //        {
        //            Count = 250
        //        },
        //        Sorting =
        //        {
        //            By = CompanySortBy.FoundedAt,
        //            Direction = SortDirection.Ascending
        //        }
        //    });

        //var results = response.Results.ToArray();

        //Assert.IsNotNull(results);
        //Assert.AreEqual(250, results.Length);

        //var result = results
        //    .Select(x => x.Company.FoundedAt)
        //    .ToArray();

        //for (var i = 1; i < result.Length; i++)
        //{
        //    Assert.IsTrue(result[i - 1] <= result[i], $"The array is not in correct ascending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        //}
    }

    [TestMethod]
    public async Task SearchCompaniesByFoundedAtAndSortingByFoundedAtDescendingTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //var from = new DateOnly(2010, 1, 1);
        //var to = new DateOnly(2020, 12, 31);

        //var response = await this.DanishCvrService
        //    .SearchCompaniesAsync(new SearchCompaniesRequest
        //    {
        //        Criteria =
        //        {
        //            FoundedAt = new Period
        //            {
        //                From = from,
        //                To = to
        //            }
        //        },
        //        Paging =
        //        {
        //            Count = 250
        //        },
        //        Sorting =
        //        {
        //            By = CompanySortBy.FoundedAt,
        //            Direction = SortDirection.Descending
        //        }
        //    });

        //var results = response.Results.ToArray();

        //Assert.IsNotNull(results);
        //Assert.AreEqual(250, results.Length);

        //var result = results
        //    .Select(x => x.Company.FoundedAt)
        //    .ToArray();

        //for (var i = 1; i < result.Length; i++)
        //{
        //    Assert.IsTrue(result[i - 1] >= result[i], $"The array is not in correct descending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        //}
    }

    [TestMethod]
    public async Task SearchCompaniesByDissolvedAtTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //var from = new DateOnly(2010, 1, 1);
        //var to = new DateOnly(2020, 12, 31);

        //var response = await this.DanishCvrService
        //    .SearchCompaniesAsync(new SearchCompaniesRequest
        //    {
        //        Criteria =
        //        {
        //            DissolvedAt = new Period
        //            {
        //                From = from,
        //                To = to
        //            }
        //        },
        //        Paging =
        //        {
        //            Count = 250
        //        }
        //    });

        //var results = response.Results.ToArray();

        //Assert.IsNotNull(results);
        //Assert.AreEqual(250, results.Length);

        //var all = results.All(x => x.Company.DissolvedAt >= from && x.Company.DissolvedAt <= to);
        //Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByDissolvedAtAndSortingByDissolvedAtAscendingTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //var from = new DateOnly(2010, 1, 1);
        //var to = new DateOnly(2020, 12, 31);

        //var response = await this.DanishCvrService
        //    .SearchCompaniesAsync(new SearchCompaniesRequest
        //    {
        //        Criteria =
        //        {
        //            DissolvedAt = new Period
        //            {
        //                From = from,
        //                To = to
        //            }
        //        },
        //        Paging =
        //        {
        //            Count = 250
        //        }
        //    });

        //var results = response.Results.ToArray();

        //Assert.IsNotNull(results);
        //Assert.AreEqual(250, results.Length);

        //var result = results
        //    .Select(x => x.Company.DissolvedAt)
        //    .ToArray();

        //for (var i = 1; i < result.Length; i++)
        //{
        //    Assert.IsTrue(result[i - 1] <= result[i], $"The array is not in correct ascending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        //}
    }

    [TestMethod]
    public async Task SearchCompaniesByDissolvedAtAndSortingByDissolvedAtDescendingTest()
    {
        await Task.CompletedTask;

        Assert.Inconclusive();

        //var from = new DateOnly(2010, 1, 1);
        //var to = new DateOnly(2020, 12, 31);

        //var response = await this.DanishCvrService
        //    .SearchCompaniesAsync(new SearchCompaniesRequest
        //    {
        //        Criteria =
        //        {
        //            DissolvedAt = new Period
        //            {
        //                From = from,
        //                To = to
        //            }
        //        },
        //        Paging =
        //        {
        //            Count = 250
        //        }
        //    });

        //var results = response.Results.ToArray();

        //Assert.IsNotNull(results);
        //Assert.AreEqual(250, results.Length);

        //var result = results
        //    .Select(x => x.Company.DissolvedAt)
        //    .ToArray();

        //for (var i = 1; i < result.Length; i++)
        //{
        //    Assert.IsTrue(result[i - 1] >= result[i], $"The array is not in correct descending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        //}
    }

    [TestMethod]
    public async Task SearchCompaniesByIsActiveTest()
    {
        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    IsActive = true
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.IsActive);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByIsActiveWhenFalseTest()
    {
        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    IsActive = false
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => !x.Company.IsActive);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByIsProtectedFromAdvertismentTest()
    {
        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    IsProtectedFromAdvertisment = true
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => x.Company.IsProtectedFromAdvertisement.GetValueOrDefault());
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByIsProtectedFromAdvertismentWhenFalseTest()
    {
        var response = await this.DanishCvrService
            .SearchCompaniesAsync(new SearchCompaniesRequest
            {
                Criteria =
                {
                    IsProtectedFromAdvertisment = false
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var all = results.All(x => !x.Company.IsProtectedFromAdvertisement.GetValueOrDefault());
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchCompaniesByRegistrationNumberPrefixTest()
    {
        const string CVR_NUMMER = "13612*";

        var response = await this.DanishCvrService
            .GetCompaniesAsync(CVR_NUMMER, new Paging { Count = 10 });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(10, results.Length);
    }
}