using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlexibleCatalogPoc.Models;

public class ProductVariant
{
    public string Sku { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    /// <summary>
    /// Variant-only fields (color, storageGb, size) sit at the same document level.
    /// </summary>
    [BsonExtraElements]
    public BsonDocument Extra { get; set; } = [];
}
