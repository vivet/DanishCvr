using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using DanishCvr.Extensions.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DanishCvr.Extensions.Serialization;

/// <summary>
/// Custom Elastic Search Serializer.
/// </summary>
public class CustomElasticSearchSerializer : IElasticsearchSerializer
{
    private readonly JsonSerializer jsonSerializer = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Converters =
        {
            new StringEnumConverter(),
            new NullableDateOnlyJsonConverter(),
            new NullableDateTimeOffsetJsonConverter()
        }
    };

    /// <inheritdoc />
    public object Deserialize(Type type, Stream stream)
    {
        if (type == null) 
            throw new ArgumentNullException(nameof(type));
        
        if (stream == null) 
            throw new ArgumentNullException(nameof(stream));
        
        using var streamReader = new StreamReader(stream);

        using var jsonReader = new JsonTextReader(streamReader);

        return this.jsonSerializer
            .Deserialize(jsonReader, type);
    }

    /// <inheritdoc />
    public T Deserialize<T>(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        return (T)this.Deserialize(typeof(T), stream);
    }

    /// <inheritdoc />
    public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
    {
        if (stream == null) 
            throw new ArgumentNullException(nameof(stream));
        
        using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true);
        
        using var jsonWriter = new JsonTextWriter(streamWriter);

        jsonWriter.Formatting = formatting == SerializationFormatting.Indented 
            ? Formatting.Indented 
            : Formatting.None;
        
        this.jsonSerializer
            .Serialize(jsonWriter, data);
        
        jsonWriter
            .Flush();
    }

    /// <inheritdoc />
    public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.Indented, CancellationToken cancellationToken = default)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        throw new NotImplementedException();
    }
}