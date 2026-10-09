using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DanishCvr.Requests.Models.Enums;
using DanishCvr.Requests;
using DanishCvr.Types;

namespace Tests.DanishCvr;

public partial class DanishCvrServiceTests
{
    [TestMethod]
    public async Task SearchPersonsByNameTest()
    {
        const string NAME = "Michael";

        var response = await this.DanishCvrService
            .SearchPersonsAsync(new SearchPersonsRequest
            {
                Criteria =
                {
                    Name = NAME
                },
                Paging =
                {
                    Count = 250
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(250, results.Length);
    }

    [TestMethod]
    public async Task SearchPersonsByNameAndSortByNameAscendingTest()
    {
        const string NAME = "Michael";

        var response = await this.DanishCvrService
            .SearchPersonsAsync(new SearchPersonsRequest
            {
                Criteria =
                {
                    Name = NAME
                },
                Sorting =
                {
                    By = PersonSortBy.Name
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(25, results.Length);

        var result = results
            .Select(x => x.Person.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) <= 0, $"The list is not in correct order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task SearchPersonsByNameAndSortByNameDescendingTest()
    {
        const string NAME = "Michael";

        var response = await this.DanishCvrService
            .SearchPersonsAsync(new SearchPersonsRequest
            {
                Criteria =
                {
                    Name = NAME
                },
                Paging =
                {
                    Skip = 500
                },
                Sorting =
                {
                    By = PersonSortBy.Name,
                    Direction = SortDirection.Descending
                }
            });

        var results = response.Results.ToArray();

        Assert.IsNotNull(results);
        Assert.AreEqual(25, results.Length);

        var result = results
            .Select(x => x.Person.Name.Value)
            .ToArray();

        for (var i = 1; i < result.Length; i++)
        {
            var previous = result[i - 1].Trim();
            var current = result[i].Trim();

            Assert.IsTrue(string.Compare(previous, current, StringComparison.OrdinalIgnoreCase) >= 0, $"The list is not in correct descending order at index {i - 1} ('{previous}') and {i} ('{current}').");
        }
    }

    [TestMethod]
    public async Task SearchPersonsByWithinTest()
    {
        var response = await this.DanishCvrService
            .SearchPersonsAsync(new SearchPersonsRequest
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
        Assert.IsTrue(results.Length > 140);
        Assert.IsTrue(results.Length < 170);
    }
}