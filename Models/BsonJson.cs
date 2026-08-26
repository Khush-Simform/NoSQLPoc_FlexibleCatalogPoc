using System.Text.Json;
using MongoDB.Bson;

namespace FlexibleCatalogPoc.Models;

public static class BsonJson
{
    public static BsonDocument FromJsonElement(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return [];
        }

        return BsonDocument.Parse(element.GetRawText());
    }

    public static BsonDocument FromObject(object? value)
    {
        if (value is null)
        {
            return [];
        }

        if (value is BsonDocument doc)
        {
            return doc;
        }

        if (value is JsonElement json)
        {
            return FromJsonElement(json);
        }

        var jsonText = JsonSerializer.Serialize(value);
        return string.IsNullOrWhiteSpace(jsonText) || jsonText == "null"
            ? []
            : BsonDocument.Parse(jsonText);
    }

    public static object? ToClr(BsonValue value) => value.BsonType switch
    {
        BsonType.Document => ToDictionary(value.AsBsonDocument),
        BsonType.Array => value.AsBsonArray.Select(ToClr).ToList(),
        BsonType.Boolean => value.AsBoolean,
        BsonType.Int32 => value.AsInt32,
        BsonType.Int64 => value.AsInt64,
        BsonType.Double => value.AsDouble,
        BsonType.Decimal128 => (decimal)value.AsDecimal128,
        BsonType.String => value.AsString,
        BsonType.Null => null,
        BsonType.DateTime => value.ToUniversalTime(),
        _ => value.ToString()
    };

    public static Dictionary<string, object?> ToDictionary(BsonDocument document)
    {
        var result = new Dictionary<string, object?>();
        foreach (var element in document)
        {
            result[element.Name] = ToClr(element.Value);
        }

        return result;
    }

    public static IEnumerable<(string Key, string Value)> DisplayPairs(BsonDocument? document)
    {
        if (document is null)
        {
            yield break;
        }

        foreach (var element in document)
        {
            yield return (ToLabel(element.Name), Format(element.Value));
        }
    }

    public static string ToLabel(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        return char.ToUpperInvariant(key[0]) + key[1..];
    }

    public static string Format(BsonValue value) => value.BsonType switch
    {
        BsonType.Boolean => value.AsBoolean ? "Yes" : "No",
        BsonType.Int32 => value.AsInt32.ToString(),
        BsonType.Int64 => value.AsInt64.ToString(),
        BsonType.Double => value.AsDouble.ToString("0.##"),
        BsonType.Decimal128 => ((decimal)value.AsDecimal128).ToString("0.##"),
        BsonType.String => value.AsString,
        _ => value.ToString() ?? string.Empty
    };
}
