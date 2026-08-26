using FlexibleCatalogPoc.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlexibleCatalogPoc.Data;

public class CatalogSeeder
{
    private readonly MongoContext _mongo;
    private readonly ILogger<CatalogSeeder> _logger;

    public CatalogSeeder(MongoContext mongo, ILogger<CatalogSeeder> logger)
    {
        _mongo = mongo;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_mongo.IsConfigured)
        {
            _logger.LogWarning("MongoDB connection string is still a placeholder. Skipping seed. See README.md.");
            return;
        }

        await EnsureIndexesAsync(cancellationToken);

        var existing = await _mongo.Products.CountDocumentsAsync(FilterDefinition<Product>.Empty, cancellationToken: cancellationToken);
        if (existing > 0)
        {
            _logger.LogInformation("Catalog already has {Count} products; skipping seed.", existing);
            return;
        }

        await _mongo.Products.InsertManyAsync(BuildProducts(), cancellationToken: cancellationToken);
        _logger.LogInformation("Seeded {Count} products across electronics, furniture, and apparel.", 7);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        await _mongo.Products.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Sku),
                new CreateIndexOptions { Unique = true, Name = "ux_sku" }),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Category),
                new CreateIndexOptions { Name = "ix_category" }),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Tags),
                new CreateIndexOptions { Name = "ix_tags" }),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Sku).Text(p => p.Brand).Text(p => p.Tags),
                new CreateIndexOptions { Name = "tx_catalog" })
        ], cancellationToken);

        await _mongo.Carts.Indexes.CreateOneAsync(
            new CreateIndexModel<Cart>(
                Builders<Cart>.IndexKeys.Ascending(c => c.UserId),
                new CreateIndexOptions { Unique = true, Name = "ux_userId" }),
            cancellationToken: cancellationToken);
    }

    private static List<Product> BuildProducts() =>
    [
        new Product
        {
            Sku = "PHONE-001",
            Name = "Acme Phone 12",
            Category = "electronics",
            Brand = "Acme",
            Price = 499,
            Attributes = new BsonDocument
            {
                { "ramGb", 8 },
                { "storageGb", 128 },
                { "os", "Android" },
                { "screenInches", 6.1 }
            },
            Variants =
            [
                Variant("PHONE-001-BLK-128", 499, 12, ("color", "black"), ("storageGb", 128)),
                Variant("PHONE-001-BLU-256", 599, 4, ("color", "blue"), ("storageGb", 256))
            ],
            Reviews =
            [
                Review("Sam", 5, "Great battery and a bright screen."),
                Review("Riley", 4, "Camera is solid for the price.")
            ],
            Tags = ["phone", "android", "5g"]
        },
        new Product
        {
            Sku = "PHONE-002",
            Name = "Acme Phone 12 Pro",
            Category = "electronics",
            Brand = "Acme",
            Price = 799,
            Attributes = new BsonDocument
            {
                { "ramGb", 12 },
                { "storageGb", 256 },
                { "os", "Android" },
                { "screenInches", 6.7 },
                { "refreshHz", 120 }
            },
            Variants =
            [
                Variant("PHONE-002-GRY-256", 799, 8, ("color", "graphite"), ("storageGb", 256)),
                Variant("PHONE-002-GRY-512", 949, 3, ("color", "graphite"), ("storageGb", 512))
            ],
            Reviews = [Review("Jordan", 5, "The 120Hz display is worth it.")],
            Tags = ["phone", "android", "flagship"]
        },
        new Product
        {
            Sku = "LAPTOP-014",
            Name = "Nova Laptop 14",
            Category = "electronics",
            Brand = "Nova",
            Price = 1099,
            Attributes = new BsonDocument
            {
                { "ramGb", 16 },
                { "storageGb", 512 },
                { "cpu", "Nova 8-core" },
                { "screenInches", 14.0 },
                { "weightKg", 1.3 }
            },
            Variants =
            [
                Variant("LAPTOP-014-SLV-512", 1099, 6, ("color", "silver"), ("storageGb", 512)),
                Variant("LAPTOP-014-SLV-1024", 1299, 2, ("color", "silver"), ("storageGb", 1024))
            ],
            Reviews = [Review("Avery", 4, "Light enough for daily commute.")],
            Tags = ["laptop", "work"]
        },
        new Product
        {
            Sku = "SOFA-301",
            Name = "Cloud Sofa 3-seater",
            Category = "furniture",
            Brand = "Harbor Home",
            Price = 1299,
            Attributes = new BsonDocument
            {
                { "fabric", "linen" },
                { "widthCm", 220 },
                { "depthCm", 95 },
                { "seats", 3 },
                { "color", "sand" }
            },
            Variants =
            [
                Variant("SOFA-301-SAND", 1299, 5, ("color", "sand"), ("fabric", "linen")),
                Variant("SOFA-301-SLATE", 1349, 2, ("color", "slate"), ("fabric", "velvet"))
            ],
            Reviews = [Review("Chris", 5, "Deep seats. The fabric feels expensive.")],
            Tags = ["sofa", "living-room"]
        },
        new Product
        {
            Sku = "TABLE-410",
            Name = "Oak Dining Table",
            Category = "furniture",
            Brand = "Harbor Home",
            Price = 890,
            Attributes = new BsonDocument
            {
                { "material", "oak" },
                { "widthCm", 180 },
                { "seats", 6 },
                { "finish", "matte oil" }
            },
            Variants =
            [
                Variant("TABLE-410-180", 890, 4, ("widthCm", 180), ("seats", 6)),
                Variant("TABLE-410-220", 1090, 1, ("widthCm", 220), ("seats", 8))
            ],
            Reviews = [Review("Morgan", 4, "Heavy, solid, looks better in person.")],
            Tags = ["table", "dining"]
        },
        new Product
        {
            Sku = "TEE-220",
            Name = "Everyday Tee",
            Category = "apparel",
            Brand = "Thread Co",
            Price = 28,
            Attributes = new BsonDocument
            {
                { "material", "organic cotton" },
                { "fit", "regular" },
                { "weightGsm", 180 }
            },
            Variants =
            [
                Variant("TEE-220-WHT-M", 28, 20, ("color", "white"), ("size", "M")),
                Variant("TEE-220-WHT-L", 28, 14, ("color", "white"), ("size", "L")),
                Variant("TEE-220-NVY-M", 28, 9, ("color", "navy"), ("size", "M"))
            ],
            Reviews = [Review("Pat", 5, "Soft and holds shape after washing.")],
            Tags = ["tshirt", "basics"]
        },
        new Product
        {
            Sku = "JKT-088",
            Name = "Trail Jacket",
            Category = "apparel",
            Brand = "Trailform",
            Price = 149,
            Attributes = new BsonDocument
            {
                { "material", "recycled nylon" },
                { "waterproof", true },
                { "weightGrams", 380 },
                { "fit", "athletic" }
            },
            Variants =
            [
                Variant("JKT-088-GRN-S", 149, 7, ("color", "pine"), ("size", "S")),
                Variant("JKT-088-GRN-M", 149, 11, ("color", "pine"), ("size", "M")),
                Variant("JKT-088-BLK-M", 149, 5, ("color", "black"), ("size", "M"))
            ],
            Reviews = [Review("Quinn", 4, "Packed it for a wet hike. Stayed dry.")],
            Tags = ["jacket", "outdoor"]
        }
    ];

    private static ProductVariant Variant(string sku, decimal price, int stock, params (string Name, BsonValue Value)[] extra)
    {
        var document = new BsonDocument();
        foreach (var (name, value) in extra)
        {
            document[name] = value;
        }

        return new ProductVariant
        {
            Sku = sku,
            Price = price,
            Stock = stock,
            Extra = document
        };
    }

    private static ProductReview Review(string author, int rating, string comment) => new()
    {
        Author = author,
        Rating = rating,
        Comment = comment,
        CreatedAt = DateTime.UtcNow.AddDays(-rating)
    };
}
