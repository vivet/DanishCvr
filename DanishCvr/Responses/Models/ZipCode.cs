using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DanishCvr.Responses.Models;

/// <summary>
/// Zip Code.
/// </summary>
public class ZipCode
{
    /// <summary>
    /// Href.
    /// </summary>
    public virtual string Href { get; set; }

    /// <summary>
    /// Nr.
    /// </summary>
    public virtual string Nr { get; set; }

    /// <summary>
    /// Navn.
    /// </summary>
    public virtual string Navn { get; set; }

    /// <summary>
    /// Stor Modtager Adresser.
    /// </summary>
    public virtual string StorModtagerAdresser { get; set; }

    /// <summary>
    /// Bbox.
    /// </summary>
    public virtual double[] Bbox { get; set; }

    /// <summary>
    /// Visuelt Center.
    /// </summary>
    public virtual double[] VisueltCenter { get; set; }

    /// <summary>
    /// Kommuner.
    /// </summary>
    public virtual IEnumerable<ZipCodeKommune> Kommuner { get; set; } = new List<ZipCodeKommune>();

    /// <summary>
    /// Ændret.
    /// </summary>
    public virtual DateTimeOffset? Ændret { get; set; }

    /// <summary>
    /// Geo Ændret.
    /// </summary>
    [JsonProperty("geo_ændret")]
    public virtual DateTimeOffset? GeoÆndret { get; set; }

    /// <summary>
    /// Geo Version.
    /// </summary>
    [JsonProperty("geo_version")]
    public virtual int GeoVersion { get; set; }

    /// <summary>
    /// Dag Id.
    /// </summary>
    [JsonProperty("dagi_id")]
    public virtual int DagiId { get; set; }
}