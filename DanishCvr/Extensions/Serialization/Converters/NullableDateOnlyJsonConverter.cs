using System;
using Newtonsoft.Json;

namespace DanishCvr.Extensions.Serialization.Converters;

/// <summary>
/// Nullable Date Only Json Converter.
/// </summary>
public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, DateOnly? value, JsonSerializer serializer)
    {
        if (writer == null) 
            throw new ArgumentNullException(nameof(writer));
        
        if (serializer == null) 
            throw new ArgumentNullException(nameof(serializer));
        
        if (value.HasValue)
        {
            writer
                .WriteValue(value.Value);
        }
        else
        {
            writer
                .WriteNull();
        }
    }

    /// <inheritdoc />
    public override DateOnly? ReadJson(JsonReader reader, Type objectType, DateOnly? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var dateOnlyString = reader.Value?.ToString();

        var success = DateOnly.TryParse(dateOnlyString, out var dateOnly);

        if (success)
        {
            return dateOnly;
        }

        return null;
    }
}