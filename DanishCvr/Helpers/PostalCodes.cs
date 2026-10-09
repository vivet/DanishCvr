using System;
using System.Collections.Generic;
using System.Linq;
using DanishCvr.Extensions;
using DanishCvr.Responses.Models;
using DanishCvr.Types;
using NetTopologySuite.Geometries;

namespace DanishCvr.Helpers;

/// <summary>
/// Postal Codes.
/// </summary>
public static class PostalCodes
{
    private static readonly IEnumerable<ZipCode> postalCodes;

    static PostalCodes()
    {
        var assembly = typeof(PostalCodes).Assembly;

        PostalCodes.postalCodes = assembly
            .GetJsonResource<IEnumerable<ZipCode>>("DanishCvr.Resources.postnumre.json");
    }

    /// <summary>
    /// Get Postal Codes.
    /// </summary>
    /// <returns>The list of zip codes.</returns>
    public static IEnumerable<ZipCode> GetPostalCodes()
    {
        return PostalCodes.postalCodes;
    }

    /// <summary>
    /// Get Postal Codes Within.
    /// </summary>
    /// <param name="within">The <see cref="Within"/></param>
    /// <returns>The matching zip codes.</returns>
    public static IEnumerable<string> GetPostalCodesWithin(Within within)
    {
        if (within == null) 
            throw new ArgumentNullException(nameof(within));
        
        var bufferInDegrees = within.Radius / 111000.00D;
        var geometryFactory = new GeometryFactory();

        var searchPoint = geometryFactory
            .CreatePoint(new Coordinate(within.Location.Longitude, within.Location.Latitude));

        var searchArea = searchPoint
            .Buffer(bufferInDegrees);

        return postalCodes
            .Where(p =>
            {
                var linearRing = new[]
                {
                    new Coordinate(p.Bbox[0], p.Bbox[1]),
                    new Coordinate(p.Bbox[2], p.Bbox[1]),
                    new Coordinate(p.Bbox[2], p.Bbox[3]),
                    new Coordinate(p.Bbox[0], p.Bbox[3]),
                    new Coordinate(p.Bbox[0], p.Bbox[1])
                };

                var bboxPolygon = geometryFactory
                    .CreatePolygon(linearRing);

                return searchArea
                    .Intersects(bboxPolygon);
            })
            .Select(x => x.Nr);
    }
}