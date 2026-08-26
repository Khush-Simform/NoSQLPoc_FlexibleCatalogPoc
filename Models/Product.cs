using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlexibleCatalogPoc.Models;

[BsonIgnoreExtraElements]
public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Brand { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Category-specific fields live here (phone RAM vs sofa fabric vs shirt fit).
    /// A relational model would need EAV tables or per-category schemas.
    /// </summary>
    public BsonDocument Attributes { get; set; } = [];

    public List<ProductVariant> Variants { get; set; } = [];
    public List<ProductReview> Reviews { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
