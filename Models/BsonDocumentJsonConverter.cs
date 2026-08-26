using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace FlexibleCatalogPoc.Models;

public sealed class BsonDocumentJsonConverter : JsonConverter<BsonDocument>
{
    public override BsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }

        using var document = JsonDocument.ParseValue(ref reader);
        return BsonJson.FromJsonElement(document.RootElement);
    }

    public override void Write(Utf8JsonWriter writer, BsonDocument value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, BsonJson.ToDictionary(value), options);
    }
}
