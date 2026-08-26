using System.Text.Json;

namespace FlexibleCatalogPoc.Models;

public class CreateProductRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public JsonElement? Attributes { get; set; }
    public List<ProductVariant>? Variants { get; set; }
    public List<string>? Tags { get; set; }
}

public class AddReviewRequest
{
    public string Author { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class AddCartItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int Qty { get; set; } = 1;
}

public class ApplyPromoRequest
{
    public string Code { get; set; } = string.Empty;
}
