using System.Text.Json;
using FlexibleCatalogPoc.Data;
using FlexibleCatalogPoc.Models;
using MongoDB.Driver;

namespace FlexibleCatalogPoc.Services;

public class ProductService
{
    private readonly MongoContext _mongo;

    public ProductService(MongoContext mongo)
    {
        _mongo = mongo;
    }

    public Task<List<Product>> ListAsync(string? category, CancellationToken cancellationToken = default)
    {
        var filter = string.IsNullOrWhiteSpace(category)
            ? FilterDefinition<Product>.Empty
            : Builders<Product>.Filter.Eq(p => p.Category, category.Trim().ToLowerInvariant());

        return _mongo.Products.Find(filter).SortBy(p => p.Category).ThenBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _mongo.Products.Find(p => p.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Product>> SearchAsync(
        string? q,
        string? category,
        int? storageGb,
        int? ramGb,
        string? fabric,
        string? material,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<Product>>();

        if (!string.IsNullOrWhiteSpace(q))
        {
            filters.Add(Builders<Product>.Filter.Text(q.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filters.Add(Builders<Product>.Filter.Eq(p => p.Category, category.Trim().ToLowerInvariant()));
        }

        if (storageGb.HasValue)
        {
            filters.Add(Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Eq("attributes.storageGb", storageGb.Value),
                Builders<Product>.Filter.Eq("variants.storageGb", storageGb.Value)));
        }

        if (ramGb.HasValue)
        {
            filters.Add(Builders<Product>.Filter.Eq("attributes.ramGb", ramGb.Value));
        }

        if (!string.IsNullOrWhiteSpace(fabric))
        {
            filters.Add(Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Eq("attributes.fabric", fabric),
                Builders<Product>.Filter.Eq("variants.fabric", fabric)));
        }

        if (!string.IsNullOrWhiteSpace(material))
        {
            filters.Add(Builders<Product>.Filter.Eq("attributes.material", material));
        }

        var filter = filters.Count == 0
            ? FilterDefinition<Product>.Empty
            : Builders<Product>.Filter.And(filters);

        return await _mongo.Products.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Sku) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException("Sku, Name, and Category are required.");
        }

        var product = new Product
        {
            Sku = request.Sku.Trim(),
            Name = request.Name.Trim(),
            Category = request.Category.Trim().ToLowerInvariant(),
            Brand = request.Brand?.Trim(),
            Price = request.Price,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency,
            Attributes = request.Attributes is { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null } attributes
                ? BsonJson.FromJsonElement(attributes)
                : [],
            Variants = request.Variants ?? [],
            Tags = request.Tags ?? [],
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _mongo.Products.InsertOneAsync(product, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException($"SKU '{product.Sku}' already exists.");
        }

        return product;
    }

    public async Task<Product?> AddReviewAsync(string id, AddReviewRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        var review = new ProductReview
        {
            Author = string.IsNullOrWhiteSpace(request.Author) ? "Anonymous" : request.Author.Trim(),
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var update = Builders<Product>.Update.Push(p => p.Reviews, review);
        var options = new FindOneAndUpdateOptions<Product> { ReturnDocument = ReturnDocument.After };
        return await _mongo.Products.FindOneAndUpdateAsync(p => p.Id == id, update, options, cancellationToken);
    }
}
