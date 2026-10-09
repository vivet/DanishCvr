using System;
using Newtonsoft.Json;

namespace DanishCvr.Extensions.Serialization.Converters;

/// <summary>
/// Nullable Date Time Offset Json Converter.
/// </summary>
public class NullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, DateTimeOffset? value, JsonSerializer serializer)
    {
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
    public override DateTimeOffset? ReadJson(JsonReader reader, Type objectType, DateTimeOffset? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var dateTimeOffsetString = reader.Value?.ToString();

        var success = DateTimeOffset.TryParse(dateTimeOffsetString, out var dateTimeOffset);

        if (success)
        {
            return dateTimeOffset;
        }

        return null;
    }
}