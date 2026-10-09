using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Range = DanishCvr.Types.Range;
using DanishCvr.Types;
using DanishCvr.Requests.Models.Enums;
using DanishCvr.Requests;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task SearchProductionUnitsTest()
    {
        var responses = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
            {
                Paging = 
                {
                    Count = 250
                }
            });

        Assert.IsNotNull(responses);
        Assert.AreEqual(250, responses.Results.Count());
    }

    [TestMethod]
    public async Task SearchProductionUnitsByAllTest()
    {
        const string NAME = "11x Mod";
        string[] industryCodes = ["620100", "642020"];
        const int MIN = 0;
        const int MAX = 200;

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME
                    },
                    IndustryCodes = industryCodes,
                    NumberOfEmployees = new Range
                    {
                        From = MIN,
                        To = MAX
                    },
                    IsActive = false
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Length);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameTest()
    {
        const string NAME = "Vivet";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = NAME,
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
        Assert.AreEqual(19, results.Length);

        var all = results.All(x => x.ProductionUnit.Name.Value.ToUpper().Contains(NAME.ToUpper()));
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameWhenHistoricNameTest()
    {
        const string HISTORIC_NAME = "Darth Vivet";
        const string EXPECTED_NAME = "Vivet Software ApS";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = HISTORIC_NAME,
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
        Assert.AreEqual(EXPECTED_NAME, first.ProductionUnit.Name.Value);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameWhenHistoricNameAndNotIncludedTest()
    {
        const string HISTORIC_NAME = "Darth Vivet";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
            {
                Criteria =
                {
                    Names =
                    {
                        Name = HISTORIC_NAME,
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
    public async Task SearchProductionUnitsByNameAndWhitespaceTest()
    {
        const string NAME = "11x Mod";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => x.ProductionUnit.Name.Value.ToUpper().Contains(NAME.ToUpper()));
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameAndIncompletePhraseTest()
    {
        const string NAME = "microsa";
        const string EXPECTED_NAME = "MICROSAP";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
        Assert.AreEqual(EXPECTED_NAME, first.ProductionUnit.Name.Value);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameAndIncompletePhraseAndMultipleWordsTest()
    {
        const string NAME_PART_ONE = "Vivet";
        const string NAME_PART_TWO = "Softw";
        const string NAME = $"{NAME_PART_ONE} {NAME_PART_TWO}";
        const string EXPECTED_NAME = "Vivet Software ApS";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
        Assert.AreEqual(EXPECTED_NAME, first.ProductionUnit.Name.Value);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameAndSortByNameAscendingTest()
    {
        const string NAME = "Microsoft";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
                    By = ProductionUnitSortBy.Name
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(22, results.Length);

        var result = results
            .Select(x => x.ProductionUnit.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) <= 0, $"The list is not in correct order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNameAndSortByNameDescendingTest()
    {
        const string NAME = "Microsoft";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
                    By = ProductionUnitSortBy.Name,
                    Direction = SortDirection.Descending
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(22, results.Length);

        var result = results
            .Select(x => x.ProductionUnit.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) >= 0, $"The list is not in correct descending order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task SearchProductionUnitsByWithinTest()
    {
        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
                    Count = 800
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.IsTrue(results.Length > 600);
        Assert.IsTrue(results.Length < 800);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByIndustryCodesTest()
    {
        string[] industryCodes = ["620100", "642020"];

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => x.ProductionUnit.Industry?.Code is "620100" or "642020");
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByIndustryCodesWhenSecondaryIndustryTest()
    {
        const string EXPECTED_NAME = "MICROSOFT DANMARK APS";

        string[] industryCodes = ["461890"];

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
        Assert.AreEqual(EXPECTED_NAME, first.ProductionUnit.Name.Value);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByStatusTest()
    {
        const string STATUS = "Normal";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => x.ProductionUnit.Status.Current.Value == STATUS);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByStatusAndInactiveTest()
    {
        const string STATUS = "Ophørt";

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => x.ProductionUnit.Status.Current.Value == STATUS);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNumberOfEmployeesTest()
    {
        const int MIN = 10;
        const int MAX = 20;

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => x.ProductionUnit.Employees == null || x.ProductionUnit.Employees.NumberOfEmployees is >= MIN and <= MAX);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNumberOfEmployeesAndSortingByNumberOfEmployeesAscendingTest()
    {
        const int MIN = 10;
        const int MAX = 20;

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
                    By = ProductionUnitSortBy.NumberOfEmployees
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var result = results
            .Select(x => x.ProductionUnit.Employees.NumberOfEmployees)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            Assert.IsTrue(result[i - 1] <= result[i], $"The array is not in correct ascending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        }
    }

    [TestMethod]
    public async Task SearchProductionUnitsByNumberOfEmployeesAndSortingByNumberOfEmployeesDescendingTest()
    {
        const int MIN = 10;
        const int MAX = 20;

        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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
                    By = ProductionUnitSortBy.NumberOfEmployees
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);

        var result = results
            .Select(x => x.ProductionUnit.Employees?.NumberOfEmployees)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            Assert.IsTrue(result[i - 1] >= result[i], $"The array is not in correct descending order at index {i - 1} ('{result[i - 1]}') and {i} ('{result[i]}').");
        }
    }

    [TestMethod]
    public async Task SearchProductionUnitsByIsActiveTest()
    {
        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => x.ProductionUnit.Status.IsActive);
        Assert.IsTrue(all);
    }

    [TestMethod]
    public async Task SearchProductionUnitsByIsActiveWhenFalseTest()
    {
        var response = await this.DanishCvrService
            .SearchProductionUnitsAsync(new SearchProductionUnitsRequest
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

        var all = results.All(x => !x.ProductionUnit.Status.IsActive);
        Assert.IsTrue(all);
    }
}